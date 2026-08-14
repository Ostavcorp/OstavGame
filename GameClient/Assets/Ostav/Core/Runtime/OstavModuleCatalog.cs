using System;
using System.Collections.Generic;

namespace Ostav
{
    public sealed class OstavModuleCatalog : IOstavModuleCatalog
    {
        private readonly List<IOstavModuleProvider> providers =
            new List<IOstavModuleProvider>();
        private readonly Dictionary<string, IOstavModuleProvider> providersById =
            new Dictionary<string, IOstavModuleProvider>(StringComparer.Ordinal);

        public IReadOnlyCollection<IOstavModuleProvider> Providers
        {
            get { return providers.AsReadOnly(); }
        }

        public void Register(IOstavModuleProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (provider.Module == null)
            {
                throw new ArgumentException("Provider.Module is required.", "provider");
            }

            if (providersById.ContainsKey(provider.Module.Id))
            {
                throw new InvalidOperationException(
                    "A provider for module Id '" + provider.Module.Id +
                    "' is already registered.");
            }

            providersById.Add(provider.Module.Id, provider);
            providers.Add(provider);
        }

        public bool TryGetProvider(
            string moduleId,
            out IOstavModuleProvider provider)
        {
            if (moduleId == null)
            {
                provider = null;
                return false;
            }

            return providersById.TryGetValue(moduleId, out provider);
        }

        public IReadOnlyCollection<IOstavModuleProvider> FindByCapability(
            string capabilityId)
        {
            var matches = new List<IOstavModuleProvider>();

            foreach (IOstavModuleProvider provider in providers)
            {
                IOstavModuleManifest manifest = provider.Manifest;
                if (manifest == null || manifest.Capabilities == null)
                {
                    continue;
                }

                foreach (IOstavCapability capability in manifest.Capabilities)
                {
                    if (capability != null && string.Equals(
                        capability.Id,
                        capabilityId,
                        StringComparison.Ordinal))
                    {
                        matches.Add(provider);
                        break;
                    }
                }
            }

            return matches.AsReadOnly();
        }
    }
}
