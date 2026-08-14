using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavCapabilityDescriptor : IOstavCapability
    {
        IReadOnlyCollection<string> SupportedIntentTypes { get; }
        IReadOnlyCollection<string> SupportedActionTypes { get; }
        IReadOnlyCollection<IOstavPermission> RequiredPermissions { get; }
        OstavDataClassification Classification { get; }
    }
}
