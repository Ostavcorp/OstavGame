using System;

namespace Ostav
{
    public interface IOstavExecutionMetadata
    {
        string CorrelationId { get; }
        string RequestId { get; }
        string ParentRequestId { get; }
        string SourceModuleId { get; }
        DateTime RequestedAtUtc { get; }
    }

    public interface IOstavExecutionMetadataContext
    {
        IOstavExecutionMetadata Metadata { get; }
    }

    public sealed class OstavExecutionMetadata : IOstavExecutionMetadata
    {
        public OstavExecutionMetadata(string correlationId, string requestId,
            string parentRequestId, string sourceModuleId, DateTime requestedAtUtc)
        {
            CorrelationId = Require(correlationId, "correlationId");
            RequestId = Require(requestId, "requestId");
            ParentRequestId = parentRequestId;
            SourceModuleId = sourceModuleId;
            RequestedAtUtc = requestedAtUtc;
        }

        public string CorrelationId { get; private set; }
        public string RequestId { get; private set; }
        public string ParentRequestId { get; private set; }
        public string SourceModuleId { get; private set; }
        public DateTime RequestedAtUtc { get; private set; }

        private static string Require(string value, string name)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("A value is required.", name);
            return value;
        }
    }
}
