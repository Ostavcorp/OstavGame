using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavModuleManifest
    {
        string ModuleId { get; }
        string Version { get; }
        IReadOnlyCollection<IOstavCapability> Capabilities { get; }
        IReadOnlyCollection<IOstavPermission> RequiredPermissions { get; }
    }
}
