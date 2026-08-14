using System;

namespace Ostav
{
    public sealed class OstavIntent : IOstavIntent
    {
        public OstavIntent(
            string intentType,
            string targetCapabilityId,
            string locale,
            DateTime requestedAtUtc,
            IOstavPayload payload)
        {
            IntentType = Require(intentType, "intentType");
            TargetCapabilityId = Require(targetCapabilityId, "targetCapabilityId");
            Locale = Require(locale, "locale");
            RequestedAtUtc = requestedAtUtc;
            Payload = payload;
        }

        public string IntentType { get; private set; }
        public string TargetCapabilityId { get; private set; }
        public string Locale { get; private set; }
        public DateTime RequestedAtUtc { get; private set; }
        public IOstavPayload Payload { get; private set; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("A value is required.", parameterName);
            }

            return value;
        }
    }
}
