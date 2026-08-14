namespace Ostav
{
    public interface IOstavPayload
    {
        string SchemaId { get; }
        string SchemaVersion { get; }
        string ContentType { get; }
        string Data { get; }
    }
}
