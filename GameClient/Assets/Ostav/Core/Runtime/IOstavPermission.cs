namespace Ostav
{
    public interface IOstavPermission
    {
        string Id { get; }
        string Resource { get; }
        string Action { get; }
    }
}
