using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class PostgreSqlOstavAuditSink : IOstavAuditSink
    {
        private readonly OstavPostgreSqlDatabase database;public PostgreSqlOstavAuditSink(OstavPostgreSqlDatabase database){this.database=database??throw new ArgumentNullException(nameof(database));}
        public async Task WriteAsync(IOstavAuditRecord record,CancellationToken token){if(record==null)throw new ArgumentNullException(nameof(record));await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("INSERT INTO audit_records(request_id,correlation_id,identity_id,module_id,capability_id,operation_type,result_code,timestamp_utc,success) VALUES(@request,@correlation,@identity,@module,@capability,@operation,@code,@timestamp,@success)",c);Add(q,"request",record.RequestId);Add(q,"correlation",record.CorrelationId);Add(q,"identity",record.IdentityId);Add(q,"module",record.ModuleId);Add(q,"capability",record.CapabilityId);Add(q,"operation",record.OperationType);Add(q,"code",record.ResultCode);q.Parameters.AddWithValue("timestamp",record.TimestampUtc);q.Parameters.AddWithValue("success",record.Success);await q.ExecuteNonQueryAsync(token);}
        public async Task<IReadOnlyCollection<IOstavAuditRecord>> ReadAsync(CancellationToken token){var records=new List<IOstavAuditRecord>();await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("SELECT request_id,correlation_id,identity_id,module_id,capability_id,operation_type,result_code,timestamp_utc,success FROM audit_records ORDER BY sequence",c);await using NpgsqlDataReader r=await q.ExecuteReaderAsync(token);while(await r.ReadAsync(token))records.Add(new OstavAuditRecord(Value(r,0),Value(r,1),Value(r,2),Value(r,3),Value(r,4),Value(r,5),Value(r,6),r.GetDateTime(7).ToUniversalTime(),r.GetBoolean(8)));return records.AsReadOnly();}
        private static void Add(NpgsqlCommand q,string name,string value)=>q.Parameters.Add(name,NpgsqlDbType.Text).Value=(object)value??DBNull.Value;private static string Value(NpgsqlDataReader r,int i)=>r.IsDBNull(i)?null:r.GetString(i);
    }
}
