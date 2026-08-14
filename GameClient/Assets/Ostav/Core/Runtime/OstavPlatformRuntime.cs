using System;

namespace Ostav
{
    public sealed class OstavPlatformRuntime : IOstavPlatformRuntime
    {
        public OstavPlatformRuntime(
            IOstavModuleRegistry modules,
            IOstavModuleCatalog moduleCatalog,
            IOstavIntentRouter intents,
            IOstavEventBus events,
            IOstavRuleEngine rules,
            IOstavActionDispatcher actions,
            IOstavAuthorizationService authorization,
            IOstavVersionCompatibility versions)
        {
            Modules = modules ?? throw new ArgumentNullException("modules");
            ModuleCatalog = moduleCatalog ?? throw new ArgumentNullException("moduleCatalog");
            Intents = intents ?? throw new ArgumentNullException("intents");
            Events = events ?? throw new ArgumentNullException("events");
            Rules = rules ?? throw new ArgumentNullException("rules");
            Actions = actions ?? throw new ArgumentNullException("actions");
            Authorization = authorization ?? throw new ArgumentNullException("authorization");
            Versions = versions ?? throw new ArgumentNullException("versions");
        }

        public IOstavModuleRegistry Modules { get; private set; }
        public IOstavModuleCatalog ModuleCatalog { get; private set; }
        public IOstavIntentRouter Intents { get; private set; }
        public IOstavEventBus Events { get; private set; }
        public IOstavRuleEngine Rules { get; private set; }
        public IOstavActionDispatcher Actions { get; private set; }
        public IOstavAuthorizationService Authorization { get; private set; }
        public IOstavVersionCompatibility Versions { get; private set; }
    }
}
