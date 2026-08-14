using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class PostgreSqlOstavIdempotencyStore : IOstavConcurrentIdempotencyStore
    {
        private readonly OstavPostgreSqlDatabase database;
        public PostgreSqlOstavIdempotencyStore(OstavPostgreSqlDatabase database){this.database=database??throw new ArgumentNullException(nameof(database));}

        public async Task<IOstavIdempotencyLease> AcquireAsync(string requestId,CancellationToken token)
        {
            Validate(requestId);NpgsqlConnection connection=await database.OpenAsync(token);
            try{await using NpgsqlCommand command=new NpgsqlCommand("SELECT pg_advisory_lock(hashtextextended(@request_id,0))",connection);command.Parameters.AddWithValue("request_id",requestId);await command.ExecuteNonQueryAsync(token);return new Lease(connection,requestId);}
            catch{await connection.DisposeAsync();throw;}
        }

        public async Task<IOstavActionResult> GetAsync(string requestId,CancellationToken token)
        {
            Validate(requestId);await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("SELECT success,code,message,payload_schema_id,payload_schema_version,payload_content_type,payload_data,correlation_id,parent_request_id,source_module_id,requested_utc FROM idempotency_results WHERE request_id=@request_id",c);q.Parameters.AddWithValue("request_id",requestId);await using NpgsqlDataReader r=await q.ExecuteReaderAsync(token);if(!await r.ReadAsync(token))return null;IOstavPayload payload=r.IsDBNull(3)?null:new OstavPayload(r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6));IOstavExecutionMetadata metadata=r.IsDBNull(7)?null:new OstavExecutionMetadata(r.GetString(7),requestId,r.IsDBNull(8)?null:r.GetString(8),r.IsDBNull(9)?null:r.GetString(9),r.GetDateTime(10).ToUniversalTime());return new OstavActionResult(r.GetBoolean(0),r.GetString(1),r.GetString(2),payload,metadata);
        }

        public async Task StoreAsync(string requestId,IOstavActionResult result,CancellationToken token)
        {
            Validate(requestId);if(result==null)throw new ArgumentNullException(nameof(result));IOstavExecutionMetadata metadata=(result as OstavActionResult)?.Metadata;await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("INSERT INTO idempotency_results(request_id,success,code,message,payload_schema_id,payload_schema_version,payload_content_type,payload_data,correlation_id,parent_request_id,source_module_id,requested_utc) VALUES(@request_id,@success,@code,@message,@schema_id,@schema_version,@content_type,@data,@correlation,@parent,@source,@requested) ON CONFLICT(request_id) DO NOTHING",c);q.Parameters.AddWithValue("request_id",requestId);q.Parameters.AddWithValue("success",result.Success);q.Parameters.AddWithValue("code",result.Code);q.Parameters.AddWithValue("message",result.Message);AddNullable(q,"schema_id",result.Payload?.SchemaId);AddNullable(q,"schema_version",result.Payload?.SchemaVersion);AddNullable(q,"content_type",result.Payload?.ContentType);AddNullable(q,"data",result.Payload?.Data);AddNullable(q,"correlation",metadata?.CorrelationId);AddNullable(q,"parent",metadata?.ParentRequestId);AddNullable(q,"source",metadata?.SourceModuleId);q.Parameters.Add("requested",NpgsqlDbType.TimestampTz).Value=metadata==null?(object)DBNull.Value:metadata.RequestedAtUtc;await q.ExecuteNonQueryAsync(token);
        }

        private static void AddNullable(NpgsqlCommand command,string name,string value)=>command.Parameters.Add(name,NpgsqlDbType.Text).Value=(object)value??DBNull.Value;
        private static void Validate(string id){if(string.IsNullOrEmpty(id))throw new ArgumentException("Required.",nameof(id));}

        private sealed class Lease : IOstavIdempotencyLease
        {
            private NpgsqlConnection connection;private readonly string requestId;
            public Lease(NpgsqlConnection connection,string requestId){this.connection=connection;this.requestId=requestId;}
            public async Task ReleaseAsync(CancellationToken token){NpgsqlConnection current=Interlocked.Exchange(ref connection,null);if(current==null)return;try{await using NpgsqlCommand command=new NpgsqlCommand("SELECT pg_advisory_unlock(hashtextextended(@request_id,0))",current);command.Parameters.AddWithValue("request_id",requestId);await command.ExecuteNonQueryAsync(token);}finally{await current.DisposeAsync();}}
        }
    }
}
