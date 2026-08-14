namespace Ostav
{
    public interface IOstavIdentity
    {
        string Id { get; }
        string IdentityType { get; }
        string DisplayName { get; }
    }
}
