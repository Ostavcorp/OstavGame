namespace Ostav
{
    public enum OstavDataClassification
    {
        Public,
        Internal,
        Personal,
        Sensitive,
        HighlySensitive
    }

    public interface IOstavClassifiedPayload : IOstavPayload
    {
        OstavDataClassification Classification { get; }
    }
}
