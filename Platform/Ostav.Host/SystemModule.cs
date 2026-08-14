using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ostav;

namespace Ostav.Host
{
    public sealed class SystemModuleInstaller : IOstavModuleInstaller
    {
        public string ModuleId { get { return "ostav.system"; } }
        public void Install(IOstavPlatformRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            registration.RegisterModuleProvider(new Provider());
            registration.RegisterIntentHandler(new PingHandler());
        }

        private sealed class Provider : IOstavModuleProvider, IOstavModuleLifecycle
        {
            public Provider() { Module = new Module(); Manifest = new Manifest(); }
            public IOstavModule Module { get; private set; }
            public IOstavModuleManifest Manifest { get; private set; }
            public Task StartAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
            public Task StopAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
        }
        private sealed class Module : IOstavModule
        {
            public string Id { get { return "ostav.system"; } }
            public string Version { get { return "1.0.0"; } }
            public IReadOnlyCollection<string> Capabilities { get { return new[] { "system" }; } }
        }
        private sealed class Manifest : IOstavModuleManifest
        {
            public string ModuleId { get { return "ostav.system"; } }
            public string Version { get { return "1.0.0"; } }
            public IReadOnlyCollection<IOstavCapability> Capabilities { get { return new[] { (IOstavCapability)new Capability() }; } }
            public IReadOnlyCollection<IOstavPermission> RequiredPermissions { get { return Array.Empty<IOstavPermission>(); } }
        }
        private sealed class Capability : IOstavCapabilityDescriptor
        { public string Id{get{return "system";}}public string Version{get{return "1.0.0";}}public string ModuleId{get{return "ostav.system";}}public IReadOnlyCollection<string> SupportedIntentTypes{get{return new[]{"system.ping"};}}public IReadOnlyCollection<string> SupportedActionTypes{get{return Array.Empty<string>();}}public IReadOnlyCollection<IOstavPermission> RequiredPermissions{get{return Array.Empty<IOstavPermission>();}}public OstavDataClassification Classification{get{return OstavDataClassification.Public;}} }
        private sealed class PingHandler : IOstavIntentHandler
        {
            public string CapabilityId { get { return "system"; } }
            public bool CanHandle(IOstavIntent intent) { return intent != null && intent.IntentType == "system.ping"; }
            public Task<IOstavActionResult> HandleAsync(IOstavIntent intent, IOstavExecutionContext context, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                IOstavExecutionMetadata metadata=(context as IOstavExecutionMetadataContext)?.Metadata;
                return Task.FromResult<IOstavActionResult>(new OstavActionResult(true,OstavPlatformCodes.Ok,string.Empty,
                    new OstavPayload("ostav.system.ping.response","1.0.0","application/json","{\"service\":\"ostav\",\"status\":\"ready\"}"),metadata));
            }
        }
    }

    public sealed class HostContext : IOstavExecutionContext, IOstavExecutionMetadataContext
    {
        public HostContext(IOstavExecutionMetadata metadata) { Metadata = metadata; }
        public IOstavIdentity Identity { get { return null; } }
        public string DeviceId { get { return "server"; } }
        public string Locale { get { return "en"; } }
        public string SessionId { get { return "host"; } }
        public IOstavExecutionMetadata Metadata { get; private set; }
    }

    public sealed class ExternalRequest : IOstavExternalRequest
    {
        public ExternalRequest(string requestId,string correlationId,string capability,string intentType)
        {RequestId=requestId;CorrelationId=correlationId;TargetCapabilityId=capability;IntentType=intentType;}
        public string ApiVersion{get{return "1.0";}}public string RequestId{get;private set;}public string CorrelationId{get;private set;}
        public IOstavIdentity Identity{get{return null;}}public string TargetCapabilityId{get;private set;}public string IntentType{get;private set;}
        public string Locale{get{return "en";}}public IOstavPayload Payload{get{return null;}}
    }

    public static class HostFactory
    {
        public static IOstavPlatformRuntime CreateRuntime()
        { return new OstavPlatformBuilder().Install(new SystemModuleInstaller()).Build(); }
    }
}
