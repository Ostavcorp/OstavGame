using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavModule
    {
        string Id { get; }
        string Version { get; }
        IReadOnlyCollection<string> Capabilities { get; }
    }
}
