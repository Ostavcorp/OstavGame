namespace Ostav
{
    public interface IOstavPlatformRuntime
    {
        IOstavModuleRegistry Modules { get; }
        IOstavModuleCatalog ModuleCatalog { get; }
        IOstavIntentRouter Intents { get; }
        IOstavEventBus Events { get; }
        IOstavRuleEngine Rules { get; }
        IOstavActionDispatcher Actions { get; }
        IOstavAuthorizationService Authorization { get; }
        IOstavVersionCompatibility Versions { get; }
    }
}
