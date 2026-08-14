namespace Ostav
{
    public enum OstavModuleState { Registered, Starting, Ready, Degraded, Unavailable, Stopped }
    public interface IOstavModuleHealth
    {
        string ModuleId { get; }
        OstavModuleState State { get; }
        bool IsCapabilityAvailable(string capabilityId);
    }
    public interface IOstavModuleHealthProvider : IOstavModuleProvider
    {
        IOstavModuleHealth Health { get; }
    }
}
