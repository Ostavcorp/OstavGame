using System;

namespace Ostav
{
    public interface IOstavEvent
    {
        string EventType { get; }
        string SourceModuleId { get; }
        DateTime OccurredAtUtc { get; }
    }
}
