namespace Ostav
{
    public interface IOstavExecutionContext
    {
        IOstavIdentity Identity { get; }
        string DeviceId { get; }
        string Locale { get; }
        string SessionId { get; }
    }
}
