using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavModuleCatalog
    {
        IReadOnlyCollection<IOstavModuleProvider> Providers { get; }
        void Register(IOstavModuleProvider provider);
        bool TryGetProvider(string moduleId, out IOstavModuleProvider provider);
        IReadOnlyCollection<IOstavModuleProvider> FindByCapability(string capabilityId);
    }
}
