using System;
using System.Collections.Generic;

namespace Ostav
{
    public sealed class OstavModuleRegistry : IOstavModuleRegistry
    {
        private readonly List<IOstavModule> modules = new List<IOstavModule>();
        private readonly Dictionary<string, IOstavModule> modulesById =
            new Dictionary<string, IOstavModule>(StringComparer.Ordinal);

        public IReadOnlyCollection<IOstavModule> Modules
        {
            get { return modules.AsReadOnly(); }
        }

        public void Register(IOstavModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            if (modulesById.ContainsKey(module.Id))
            {
                throw new InvalidOperationException(
                    "A module with Id '" + module.Id + "' is already registered.");
            }

            modulesById.Add(module.Id, module);
            modules.Add(module);
        }

        public bool TryGetModule(string moduleId, out IOstavModule module)
        {
            if (moduleId == null)
            {
                module = null;
                return false;
            }

            return modulesById.TryGetValue(moduleId, out module);
        }

        public IReadOnlyCollection<IOstavModule> FindByCapability(string capabilityId)
        {
            var matches = new List<IOstavModule>();

            foreach (IOstavModule module in modules)
            {
                if (ContainsCapability(module.Capabilities, capabilityId))
                {
                    matches.Add(module);
                }
            }

            return matches.AsReadOnly();
        }

        private static bool ContainsCapability(
            IReadOnlyCollection<string> capabilities,
            string capabilityId)
        {
            if (capabilities == null)
            {
                return false;
            }

            foreach (string capability in capabilities)
            {
                if (string.Equals(capability, capabilityId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
