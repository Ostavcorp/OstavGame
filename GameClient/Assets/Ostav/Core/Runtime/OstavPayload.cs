using System;

namespace Ostav
{
    public sealed class OstavPayload : IOstavPayload
    {
        public OstavPayload(
            string schemaId,
            string schemaVersion,
            string contentType,
            string data)
        {
            SchemaId = Require(schemaId, "schemaId");
            SchemaVersion = Require(schemaVersion, "schemaVersion");
            ContentType = Require(contentType, "contentType");
            Data = data ?? throw new ArgumentNullException("data");
        }

        public string SchemaId { get; private set; }
        public string SchemaVersion { get; private set; }
        public string ContentType { get; private set; }
        public string Data { get; private set; }

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
