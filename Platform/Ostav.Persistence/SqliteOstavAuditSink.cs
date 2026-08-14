using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class SqliteOstavAuditSink : IOstavAuditSink
    {
        private readonly OstavSqliteDatabase database;public SqliteOstavAuditSink(OstavSqliteDatabase database){this.database=database??throw new ArgumentNullException("database");}
        public async Task WriteAsync(IOstavAuditRecord record,CancellationToken token){if(record==null)throw new ArgumentNullException("record");await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="INSERT INTO audit_records(request_id,correlation_id,identity_id,module_id,capability_id,operation_type,result_code,timestamp_utc,success) VALUES($request,$correlation,$identity,$module,$capability,$operation,$code,$utc,$success)";command.Parameters.AddWithValue("$request",(object)record.RequestId??DBNull.Value);command.Parameters.AddWithValue("$correlation",(object)record.CorrelationId??DBNull.Value);command.Parameters.AddWithValue("$identity",(object)record.IdentityId??DBNull.Value);command.Parameters.AddWithValue("$module",(object)record.ModuleId??DBNull.Value);command.Parameters.AddWithValue("$capability",(object)record.CapabilityId??DBNull.Value);command.Parameters.AddWithValue("$operation",(object)record.OperationType??DBNull.Value);command.Parameters.AddWithValue("$code",(object)record.ResultCode??DBNull.Value);command.Parameters.AddWithValue("$utc",record.TimestampUtc.ToString("O",CultureInfo.InvariantCulture));command.Parameters.AddWithValue("$success",record.Success?1:0);await command.ExecuteNonQueryAsync(token);}
        public async Task<IReadOnlyCollection<IOstavAuditRecord>> ReadAsync(CancellationToken token){var records=new List<IOstavAuditRecord>();await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="SELECT request_id,correlation_id,identity_id,module_id,capability_id,operation_type,result_code,timestamp_utc,success FROM audit_records ORDER BY sequence";await using var reader=await command.ExecuteReaderAsync(token);while(await reader.ReadAsync(token))records.Add(new OstavAuditRecord(Value(reader,0),Value(reader,1),Value(reader,2),Value(reader,3),Value(reader,4),Value(reader,5),Value(reader,6),DateTime.Parse(reader.GetString(7),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind),reader.GetInt32(8)!=0));return records.AsReadOnly();}
        private static string Value(Microsoft.Data.Sqlite.SqliteDataReader reader,int index){return reader.IsDBNull(index)?null:reader.GetString(index);}
    }
}
