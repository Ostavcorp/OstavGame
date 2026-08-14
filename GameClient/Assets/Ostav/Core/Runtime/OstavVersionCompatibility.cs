using System;

namespace Ostav
{
    public sealed class OstavVersionCompatibility : IOstavVersionCompatibility
    {
        public bool IsCompatible(string requiredVersion, string availableVersion)
        {
            Version required;
            Version available;

            if (!Version.TryParse(requiredVersion, out required) ||
                !Version.TryParse(availableVersion, out available))
            {
                return false;
            }

            return required.Major == available.Major &&
                available.CompareTo(required) >= 0;
        }
    }
}
