using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ostav;
using Ostav.Api;
using Ostav.Host;
using Ostav.Persistence;
using Microsoft.Extensions.Configuration;

var builder=WebApplication.CreateBuilder(args);
string configuredHttpUrl=ApiHostConfiguration.ResolveHttpUrl(builder.Configuration);
if(configuredHttpUrl!=null)builder.WebHost.UseUrls(configuredHttpUrl);
builder.WebHost.ConfigureKestrel(options=>options.Limits.MaxRequestBodySize=64*1024);
builder.Services.AddSingleton<IApiRequestAuthenticator,DevelopmentApiKeyAuthenticator>();
builder.Services.AddSingleton<ApiRuntime>();
builder.Services.AddHostedService<ApiLifecycleService>();
builder.Services.AddRateLimiter(options=>options.AddFixedWindowLimiter("execute",policy=>{policy.PermitLimit=60;policy.Window=TimeSpan.FromMinutes(1);policy.QueueLimit=0;}));

var app=builder.Build();
app.UseRateLimiter();
app.MapGet("/health/live",()=>Results.Ok(new{status="live"}));
app.MapGet("/health/ready",(ApiRuntime api)=>api.IsReady?Results.Ok(new{status="ready"}):Results.StatusCode(503));

app.MapGet("/api/v1/capabilities",(HttpContext http,IApiRequestAuthenticator auth,ApiRuntime api)=>
{
    if(!Authenticated(http,auth))return Results.Unauthorized();
    var modules=new List<ModuleDiscoveryDto>();
    foreach(IOstavModuleProvider provider in api.Runtime.ModuleCatalog.Providers)
    {
        var capabilities=new List<CapabilityDto>();
        foreach(IOstavCapability capability in provider.Manifest.Capabilities)
        {
            IOstavCapabilityDescriptor descriptor=capability as IOstavCapabilityDescriptor;
            capabilities.Add(new CapabilityDto{Id=capability.Id,Version=capability.Version,
                SupportedIntentTypes=descriptor==null?Array.Empty<string>():descriptor.SupportedIntentTypes});
        }
        modules.Add(new ModuleDiscoveryDto{ModuleId=provider.Module.Id,Version=provider.Module.Version,Capabilities=capabilities});
    }
    return Results.Ok(new CapabilityDiscoveryDto{ApiVersion="1.0",Modules=modules});
});

app.MapPost("/api/v1/execute",async (ExecuteRequestDto request,HttpContext http,IApiRequestAuthenticator auth,ApiRuntime api,ILogger<Program> log)=>
{
    if(!Authenticated(http,auth))return Results.Unauthorized();
    if(request==null||request.ApiVersion!="1.0"||string.IsNullOrEmpty(request.RequestId)||
       string.IsNullOrEmpty(request.CorrelationId)||string.IsNullOrEmpty(request.TargetCapabilityId)||
       string.IsNullOrEmpty(request.IntentType))return Results.BadRequest(new{code=OstavPlatformCodes.InvalidRequest});
    var timer=Stopwatch.StartNew();
    IOstavIdentity identity=await api.GetDevelopmentIdentityAsync(http.RequestAborted);
    IOstavExternalResponse result=await api.Adapter.ExecuteAsync(request,new ApiExecutionContext(request,identity),http.RequestAborted);
    timer.Stop();
    log.LogInformation("Ostav request {RequestId} correlation {CorrelationId} capability {Capability} operation {Operation} result {Code} durationMs {Duration}",request.RequestId,request.CorrelationId,request.TargetCapabilityId,request.IntentType,result.Code,timer.ElapsedMilliseconds);
    return Results.Ok(ExecuteResponseDto.From(result));
}).RequireRateLimiting("execute");

app.Run();

static bool Authenticated(HttpContext http,IApiRequestAuthenticator auth)
{ return auth.Authenticate(http.Request.Headers["X-Ostav-Api-Key"].ToString()); }

namespace Ostav.Api
{
    public sealed class ApiRuntime
    {
        private readonly OstavSqliteDatabase database;
        private readonly OstavPostgreSqlDatabase postgreSqlDatabase;
        private readonly OstavAccountService accountService;
        private readonly string developmentSubject;
        public ApiRuntime(IConfiguration configuration)
        {
            Runtime=HostFactory.CreateRuntime();Host=new OstavPlatformHost(Runtime);
            string mode=configuration["Ostav:Persistence:Mode"]??"InMemory";
            developmentSubject=configuration["Ostav:DevelopmentIdentity:Subject"]??"development-service";
            IOstavAuditSink audit;IOstavIdempotencyStore idempotency;IOstavAccountRepository accounts;
            if(string.Equals(mode,"SQLite",StringComparison.OrdinalIgnoreCase))
            {
                string path=configuration["Ostav:Persistence:DatabasePath"];
                if(string.IsNullOrWhiteSpace(path))throw new InvalidOperationException("SQLite mode requires Ostav:Persistence:DatabasePath.");
                database=new OstavSqliteDatabase(path);StateStore=new SqliteOstavStateStore(database);audit=new SqliteOstavAuditSink(database);idempotency=new SqliteOstavIdempotencyStore(database);accounts=new SqliteOstavAccountRepository(database);
            }
            else if(string.Equals(mode,"PostgreSQL",StringComparison.OrdinalIgnoreCase))
            {
                string connection=configuration["OSTAV_POSTGRES_CONNECTION"]??configuration["Ostav:Persistence:PostgreSqlConnectionString"];
                if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("PostgreSQL mode requires OSTAV_POSTGRES_CONNECTION.");
                postgreSqlDatabase=new OstavPostgreSqlDatabase(connection);StateStore=new PostgreSqlOstavStateStore(postgreSqlDatabase);audit=new PostgreSqlOstavAuditSink(postgreSqlDatabase);idempotency=new PostgreSqlOstavIdempotencyStore(postgreSqlDatabase);accounts=new PostgreSqlOstavAccountRepository(postgreSqlDatabase);
            }
            else if(string.Equals(mode,"InMemory",StringComparison.OrdinalIgnoreCase))
            {StateStore=new InMemoryOstavStateStore();audit=new InMemoryOstavAuditSink();idempotency=new InMemoryOstavIdempotencyStore();accounts=new InMemoryOstavAccountRepository();}
            else throw new InvalidOperationException("Unsupported persistence mode.");
            accountService=new OstavAccountService(accounts,new SystemOstavClock());
            Executor=new OstavPlatformExecutor(Runtime,audit,new SystemOstavClock(),idempotency);
            Adapter=new OstavExternalRequestAdapter(Executor,new SystemOstavClock());
        }
        public IOstavPlatformRuntime Runtime{get;private set;}public OstavPlatformHost Host{get;private set;}public OstavPlatformExecutor Executor{get;private set;}public OstavExternalRequestAdapter Adapter{get;private set;}
        public IOstavStateStore StateStore{get;private set;}
        public bool IsReady{get{foreach(IOstavModuleHealth health in Host.ModuleHealth)if(health.State!=OstavModuleState.Ready)return false;return true;}}
        public async Task InitializeAsync(CancellationToken token){if(database!=null)await database.InitializeAsync(token);if(postgreSqlDatabase!=null)await postgreSqlDatabase.InitializeAsync(token);}
        public Task<OstavAccount> GetDevelopmentIdentityAsync(CancellationToken token){return accountService.GetOrCreateAsync(developmentSubject,"Developer","en",token);}
    }
    internal sealed class ApiLifecycleService : IHostedService
    {
        private readonly ApiRuntime runtime;public ApiLifecycleService(ApiRuntime runtime){this.runtime=runtime;}
        public async Task StartAsync(CancellationToken token){await runtime.InitializeAsync(token);await runtime.GetDevelopmentIdentityAsync(token);await runtime.Host.StartAsync(token);}
        public Task StopAsync(CancellationToken token){return runtime.Host.StopAsync(token);}
    }
}

public partial class Program { }
