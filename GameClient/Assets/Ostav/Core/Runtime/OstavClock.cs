using System;

namespace Ostav
{
    public interface IOstavClock { DateTime UtcNow { get; } }

    public sealed class SystemOstavClock : IOstavClock
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }

    public sealed class ManualOstavClock : IOstavClock
    {
        public ManualOstavClock(DateTime utcNow) { UtcNow = utcNow; }
        public DateTime UtcNow { get; private set; }
        public void Set(DateTime utcNow) { UtcNow = utcNow; }
    }
}
