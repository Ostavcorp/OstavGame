using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Ostav.Persistence
{
    public sealed class OstavSqliteDatabase
    {
        public const int CurrentSchemaVersion=1;
        private readonly string connectionString;
        private bool initialized;

        public OstavSqliteDatabase(string databasePath)
        {
            if(string.IsNullOrWhiteSpace(databasePath))throw new ArgumentException("Database path is required.","databasePath");
            connectionString=new SqliteConnectionStringBuilder{DataSource=databasePath,Mode=SqliteOpenMode.ReadWriteCreate,Pooling=false}.ToString();
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            await using var connection=CreateConnection();await connection.OpenAsync(token);
            await ExecuteAsync(connection,"CREATE TABLE IF NOT EXISTS schema_info (id INTEGER PRIMARY KEY CHECK(id=1), version INTEGER NOT NULL);",token);
            await using(var command=connection.CreateCommand()){command.CommandText="SELECT version FROM schema_info WHERE id=1";object value=await command.ExecuteScalarAsync(token);if(value==null){command.CommandText="INSERT INTO schema_info(id,version) VALUES(1,0)";await command.ExecuteNonQueryAsync(token);}else if(Convert.ToInt32(value)>CurrentSchemaVersion)throw new InvalidOperationException("Database schema version is newer than supported.");}
            await ExecuteAsync(connection,"CREATE TABLE IF NOT EXISTS state_values (namespace TEXT NOT NULL, key TEXT NOT NULL, value BLOB NOT NULL, PRIMARY KEY(namespace,key));",token);
            await ExecuteAsync(connection,"CREATE TABLE IF NOT EXISTS idempotency_results (request_id TEXT PRIMARY KEY, success INTEGER NOT NULL, code TEXT NOT NULL, message TEXT NOT NULL, payload_schema_id TEXT, payload_schema_version TEXT, payload_content_type TEXT, payload_data TEXT, correlation_id TEXT, parent_request_id TEXT, source_module_id TEXT, requested_utc TEXT);",token);
            await ExecuteAsync(connection,"CREATE TABLE IF NOT EXISTS audit_records (sequence INTEGER PRIMARY KEY AUTOINCREMENT, request_id TEXT, correlation_id TEXT, identity_id TEXT, module_id TEXT, capability_id TEXT, operation_type TEXT, result_code TEXT, timestamp_utc TEXT NOT NULL, success INTEGER NOT NULL);",token);
            await ExecuteAsync(connection,"CREATE TABLE IF NOT EXISTS accounts (identity_id TEXT PRIMARY KEY, auth_subject TEXT NOT NULL UNIQUE, display_name TEXT NOT NULL, preferred_locale TEXT NOT NULL, status TEXT NOT NULL, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL);",token);
            await using(var update=connection.CreateCommand()){update.CommandText="UPDATE schema_info SET version=$version WHERE id=1";update.Parameters.AddWithValue("$version",CurrentSchemaVersion);await update.ExecuteNonQueryAsync(token);}
            initialized=true;
        }

        public SqliteConnection Open()
        {
            if(!initialized)throw new InvalidOperationException("Database has not been initialized.");
            var connection=CreateConnection();connection.Open();return connection;
        }
        private SqliteConnection CreateConnection(){return new SqliteConnection(connectionString);}
        private static async Task ExecuteAsync(SqliteConnection connection,string sql,CancellationToken token){await using var command=connection.CreateCommand();command.CommandText=sql;await command.ExecuteNonQueryAsync(token);}
    }
}
