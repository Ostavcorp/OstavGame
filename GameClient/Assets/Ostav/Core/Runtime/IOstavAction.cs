namespace Ostav
{
    public interface IOstavAction
    {
        string ActionType { get; }
        string TargetCapabilityId { get; }
        IOstavPayload Payload { get; }
    }
}
