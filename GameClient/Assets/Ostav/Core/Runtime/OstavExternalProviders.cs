using System;
using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavExternalProviderDescriptor
    {
        string ProviderId { get; }
        string Version { get; }
        IReadOnlyCollection<string> Capabilities { get; }
        IReadOnlyCollection<IOstavPermission> RequiredPermissions { get; }
    }
    public interface IOstavExternalProviderCatalog
    {
        IReadOnlyCollection<IOstavExternalProviderDescriptor> Providers { get; }
        void Register(IOstavExternalProviderDescriptor provider);
        bool TryGet(string providerId,out IOstavExternalProviderDescriptor provider);
        IReadOnlyCollection<IOstavExternalProviderDescriptor> FindByCapability(string capabilityId);
    }
    public sealed class OstavExternalProviderCatalog : IOstavExternalProviderCatalog
    {
        private readonly List<IOstavExternalProviderDescriptor> providers=new List<IOstavExternalProviderDescriptor>();
        private readonly Dictionary<string,IOstavExternalProviderDescriptor> byId=new Dictionary<string,IOstavExternalProviderDescriptor>(StringComparer.Ordinal);
        public IReadOnlyCollection<IOstavExternalProviderDescriptor> Providers{get{return providers.AsReadOnly();}}
        public void Register(IOstavExternalProviderDescriptor provider){if(provider==null)throw new ArgumentNullException("provider");if(byId.ContainsKey(provider.ProviderId))throw new InvalidOperationException("Provider is already registered.");byId.Add(provider.ProviderId,provider);providers.Add(provider);}
        public bool TryGet(string providerId,out IOstavExternalProviderDescriptor provider){if(providerId==null){provider=null;return false;}return byId.TryGetValue(providerId,out provider);}
        public IReadOnlyCollection<IOstavExternalProviderDescriptor> FindByCapability(string capabilityId){var result=new List<IOstavExternalProviderDescriptor>();foreach(var provider in providers)if(provider.Capabilities!=null)foreach(var capability in provider.Capabilities)if(string.Equals(capability,capabilityId,StringComparison.Ordinal)){result.Add(provider);break;}return result.AsReadOnly();}
    }
}
