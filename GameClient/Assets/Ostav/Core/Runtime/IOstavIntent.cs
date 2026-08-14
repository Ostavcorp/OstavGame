using System;

namespace Ostav
{
    public interface IOstavIntent
    {
        string IntentType { get; }
        string TargetCapabilityId { get; }
        string Locale { get; }
        DateTime RequestedAtUtc { get; }
        IOstavPayload Payload { get; }
    }
}
