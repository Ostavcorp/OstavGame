namespace Ostav
{
    public interface IOstavVersionCompatibility
    {
        bool IsCompatible(string requiredVersion, string availableVersion);
    }
}
