using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavModuleRegistry
    {
        IReadOnlyCollection<IOstavModule> Modules { get; }
        void Register(IOstavModule module);
        bool TryGetModule(string moduleId, out IOstavModule module);
        IReadOnlyCollection<IOstavModule> FindByCapability(string capabilityId);
    }
}
