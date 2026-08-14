namespace Ostav
{
    public interface IOstavActionResult
    {
        bool Success { get; }
        string Code { get; }
        string Message { get; }
        IOstavPayload Payload { get; }
    }
}
