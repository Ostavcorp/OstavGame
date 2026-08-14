using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavSecuredAction : IOstavAction
    {
        IReadOnlyCollection<IOstavPermission> RequiredPermissions { get; }
    }
}
