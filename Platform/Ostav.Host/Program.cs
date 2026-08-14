using System;
using System.Threading;
using System.Threading.Tasks;
using Ostav;

namespace Ostav.Host
{
    internal static class Program
    {
        private static async Task Main()
        {
            IOstavPlatformRuntime runtime=HostFactory.CreateRuntime();
            var host=new OstavPlatformHost(runtime);
            await host.StartAsync(CancellationToken.None);
            var metadata=new OstavExecutionMetadata("demo-correlation","demo-request",null,"ostav.host",DateTime.UtcNow);
            var adapter=new OstavExternalRequestAdapter(new OstavPlatformExecutor(runtime),new SystemOstavClock());
            IOstavExternalResponse response=await adapter.ExecuteAsync(new ExternalRequest("demo-request","demo-correlation","system","system.ping"),new HostContext(metadata),CancellationToken.None);
            Console.WriteLine(response.Code);
            await host.StopAsync(CancellationToken.None);
        }
    }
}
