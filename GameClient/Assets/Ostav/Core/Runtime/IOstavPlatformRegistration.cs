namespace Ostav
{
    public interface IOstavPlatformRegistration
    {
        void RegisterModuleProvider(IOstavModuleProvider provider);
        void RegisterIntentHandler(IOstavIntentHandler handler);
        void RegisterEventHandler(IOstavEventHandler handler);
        void RegisterRule(IOstavRule rule);
        void RegisterActionHandler(IOstavActionHandler handler);
    }
}
