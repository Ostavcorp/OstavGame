using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class SqliteOstavIdempotencyStore : IOstavIdempotencyStore
    {
        private readonly OstavSqliteDatabase database;public SqliteOstavIdempotencyStore(OstavSqliteDatabase database){this.database=database??throw new ArgumentNullException("database");}
        public async Task<IOstavActionResult> GetAsync(string requestId,CancellationToken token)
        {
            Validate(requestId);await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="SELECT success,code,message,payload_schema_id,payload_schema_version,payload_content_type,payload_data,correlation_id,parent_request_id,source_module_id,requested_utc FROM idempotency_results WHERE request_id=$id";command.Parameters.AddWithValue("$id",requestId);await using var reader=await command.ExecuteReaderAsync(token);if(!await reader.ReadAsync(token))return null;
            IOstavPayload payload=reader.IsDBNull(3)?null:new OstavPayload(reader.GetString(3),reader.GetString(4),reader.GetString(5),reader.GetString(6));
            IOstavExecutionMetadata metadata=reader.IsDBNull(7)?null:new OstavExecutionMetadata(reader.GetString(7),requestId,reader.IsDBNull(8)?null:reader.GetString(8),reader.IsDBNull(9)?null:reader.GetString(9),DateTime.Parse(reader.GetString(10),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind));
            return new OstavActionResult(reader.GetInt32(0)!=0,reader.GetString(1),reader.GetString(2),payload,metadata);
        }
        public async Task StoreAsync(string requestId,IOstavActionResult result,CancellationToken token)
        {
            Validate(requestId);if(result==null)throw new ArgumentNullException("result");var concrete=result as OstavActionResult;IOstavExecutionMetadata metadata=concrete?.Metadata;
            await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="INSERT OR IGNORE INTO idempotency_results(request_id,success,code,message,payload_schema_id,payload_schema_version,payload_content_type,payload_data,correlation_id,parent_request_id,source_module_id,requested_utc) VALUES($id,$success,$code,$message,$sid,$sv,$ct,$data,$correlation,$parent,$source,$utc)";
            command.Parameters.AddWithValue("$id",requestId);command.Parameters.AddWithValue("$success",result.Success?1:0);command.Parameters.AddWithValue("$code",result.Code);command.Parameters.AddWithValue("$message",result.Message);command.Parameters.AddWithValue("$sid",(object)result.Payload?.SchemaId??DBNull.Value);command.Parameters.AddWithValue("$sv",(object)result.Payload?.SchemaVersion??DBNull.Value);command.Parameters.AddWithValue("$ct",(object)result.Payload?.ContentType??DBNull.Value);command.Parameters.AddWithValue("$data",(object)result.Payload?.Data??DBNull.Value);command.Parameters.AddWithValue("$correlation",(object)metadata?.CorrelationId??DBNull.Value);command.Parameters.AddWithValue("$parent",(object)metadata?.ParentRequestId??DBNull.Value);command.Parameters.AddWithValue("$source",(object)metadata?.SourceModuleId??DBNull.Value);command.Parameters.AddWithValue("$utc",metadata==null?(object)DBNull.Value:metadata.RequestedAtUtc.ToString("O",CultureInfo.InvariantCulture));await command.ExecuteNonQueryAsync(token);
        }
        private static void Validate(string id){if(string.IsNullOrEmpty(id))throw new ArgumentException("Required.","requestId");}
    }
}
