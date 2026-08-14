namespace Ostav
{
    public interface IOstavModuleInstaller
    {
        string ModuleId { get; }
        void Install(IOstavPlatformRegistration registration);
    }
}
