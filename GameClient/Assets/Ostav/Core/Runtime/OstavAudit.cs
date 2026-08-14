using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavAuditRecord
    {
        string RequestId { get; }
        string CorrelationId { get; }
        string IdentityId { get; }
        string ModuleId { get; }
        string CapabilityId { get; }
        string OperationType { get; }
        string ResultCode { get; }
        DateTime TimestampUtc { get; }
        bool Success { get; }
    }

    public interface IOstavAuditSink
    {
        Task WriteAsync(IOstavAuditRecord record, CancellationToken cancellationToken);
    }

    public sealed class OstavAuditRecord : IOstavAuditRecord
    {
        public OstavAuditRecord(string requestId, string correlationId, string identityId,
            string moduleId, string capabilityId, string operationType, string resultCode,
            DateTime timestampUtc, bool success)
        {
            RequestId = requestId; CorrelationId = correlationId; IdentityId = identityId;
            ModuleId = moduleId; CapabilityId = capabilityId; OperationType = operationType;
            ResultCode = resultCode; TimestampUtc = timestampUtc; Success = success;
        }
        public string RequestId { get; private set; }
        public string CorrelationId { get; private set; }
        public string IdentityId { get; private set; }
        public string ModuleId { get; private set; }
        public string CapabilityId { get; private set; }
        public string OperationType { get; private set; }
        public string ResultCode { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public bool Success { get; private set; }
    }

    public sealed class InMemoryOstavAuditSink : IOstavAuditSink
    {
        private readonly List<IOstavAuditRecord> records = new List<IOstavAuditRecord>();
        public IReadOnlyCollection<IOstavAuditRecord> Records { get { return records.AsReadOnly(); } }
        public Task WriteAsync(IOstavAuditRecord record, CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException("record");
            cancellationToken.ThrowIfCancellationRequested(); records.Add(record);
            return Task.FromResult(0);
        }
    }
}
