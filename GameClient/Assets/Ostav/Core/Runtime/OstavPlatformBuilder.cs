using System;

namespace Ostav
{
    public sealed class OstavPlatformBuilder : IOstavPlatformRegistration
    {
        private readonly OstavModuleRegistry modules = new OstavModuleRegistry();
        private readonly OstavModuleCatalog moduleCatalog = new OstavModuleCatalog();
        private readonly OstavIntentRouter intents = new OstavIntentRouter();
        private readonly OstavEventBus events = new OstavEventBus();
        private readonly OstavRuleEngine rules = new OstavRuleEngine();
        private readonly InMemoryOstavConsentStore consentStore;
        private readonly OstavConsentService consentService;
        private readonly OstavAuthorizationService authorization;
        private readonly OstavActionDispatcher actions;

        public OstavPlatformBuilder()
        {
            consentStore = new InMemoryOstavConsentStore();
            consentService = new OstavConsentService(consentStore);
            authorization = new OstavAuthorizationService(consentService);
            actions = new OstavActionDispatcher(authorization);
        }

        public OstavPlatformBuilder Register(IOstavModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            modules.Register(module);
            return this;
        }

        public OstavPlatformBuilder Register(IOstavIntentHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            intents.Register(handler);
            return this;
        }

        public OstavPlatformBuilder RegisterModuleProvider(
            IOstavModuleProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            modules.Register(provider.Module);
            moduleCatalog.Register(provider);
            return this;
        }

        public OstavPlatformBuilder Register(IOstavEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            events.Subscribe(handler);
            return this;
        }

        public OstavPlatformBuilder Register(IOstavRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            rules.Register(rule);
            return this;
        }

        public OstavPlatformBuilder Register(IOstavActionHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            actions.Register(handler);
            return this;
        }

        public OstavPlatformBuilder Install(IOstavModuleInstaller installer)
        {
            if (installer == null)
            {
                throw new ArgumentNullException("installer");
            }

            installer.Install(this);
            return this;
        }

        void IOstavPlatformRegistration.RegisterModuleProvider(
            IOstavModuleProvider provider)
        {
            RegisterModuleProvider(provider);
        }

        void IOstavPlatformRegistration.RegisterIntentHandler(
            IOstavIntentHandler handler)
        {
            Register(handler);
        }

        void IOstavPlatformRegistration.RegisterEventHandler(
            IOstavEventHandler handler)
        {
            Register(handler);
        }

        void IOstavPlatformRegistration.RegisterRule(IOstavRule rule)
        {
            Register(rule);
        }

        void IOstavPlatformRegistration.RegisterActionHandler(
            IOstavActionHandler handler)
        {
            Register(handler);
        }

        public IOstavPlatformRuntime Build()
        {
            ValidateDependencies();
            return new OstavPlatformRuntime(
                modules,
                moduleCatalog,
                intents,
                events,
                rules,
                actions,
                authorization,
                new OstavVersionCompatibility());
        }

        private void ValidateDependencies()
        {
            var compatibility = new OstavVersionCompatibility();
            foreach (IOstavModuleProvider provider in moduleCatalog.Providers)
            {
                IOstavDependencyManifest manifest = provider.Manifest as IOstavDependencyManifest;
                if (manifest == null || manifest.Dependencies == null) continue;
                foreach (IOstavModuleDependency dependency in manifest.Dependencies)
                {
                    if (!modules.TryGetModule(dependency.ModuleId, out IOstavModule module))
                        throw new OstavModuleValidationException(
                            OstavPlatformCodes.ModuleDependencyMissing,
                            "A required module is missing.");
                    if (!compatibility.IsCompatible(dependency.MinimumVersion, module.Version))
                        throw new OstavModuleValidationException(
                            OstavPlatformCodes.ModuleVersionIncompatible,
                            "A required module version is incompatible.");
                }
            }
        }
    }
}
