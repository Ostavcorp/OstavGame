using System;

namespace Ostav
{
    public sealed class OstavActionResult : IOstavActionResult
    {
        public OstavActionResult(
            bool success,
            string code,
            string message,
            IOstavPayload payload = null,
            IOstavExecutionMetadata metadata = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("A value is required.", "code");
            }

            Success = success;
            Code = code;
            Message = message ?? throw new ArgumentNullException("message");
            Payload = payload;
            Metadata = metadata;
        }

        public bool Success { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public IOstavPayload Payload { get; private set; }
        public IOstavExecutionMetadata Metadata { get; private set; }
    }
}
