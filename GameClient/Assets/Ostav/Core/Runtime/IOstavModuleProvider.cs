namespace Ostav
{
    public interface IOstavModuleProvider
    {
        IOstavModule Module { get; }
        IOstavModuleManifest Manifest { get; }
    }
}
