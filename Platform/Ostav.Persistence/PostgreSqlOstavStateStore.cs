using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class PostgreSqlOstavStateStore : IOstavStateStore
    {
        private readonly OstavPostgreSqlDatabase database;
        public PostgreSqlOstavStateStore(OstavPostgreSqlDatabase database){this.database=database??throw new ArgumentNullException(nameof(database));}
        public async Task<byte[]> GetAsync(string n,string key,CancellationToken token){Validate(n,key);await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("SELECT value FROM state_values WHERE namespace=@namespace AND key=@key",c);q.Parameters.AddWithValue("namespace",n);q.Parameters.AddWithValue("key",key);object value=await q.ExecuteScalarAsync(token);return value==null?null:(byte[])value;}
        public async Task SetAsync(string n,string key,byte[] value,CancellationToken token){Validate(n,key);if(value==null)throw new ArgumentNullException(nameof(value));await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("INSERT INTO state_values(namespace,key,value) VALUES(@namespace,@key,@value) ON CONFLICT(namespace,key) DO UPDATE SET value=excluded.value",c);q.Parameters.AddWithValue("namespace",n);q.Parameters.AddWithValue("key",key);q.Parameters.Add("value",NpgsqlDbType.Bytea).Value=value;await q.ExecuteNonQueryAsync(token);}
        public async Task RemoveAsync(string n,string key,CancellationToken token){Validate(n,key);await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("DELETE FROM state_values WHERE namespace=@namespace AND key=@key",c);q.Parameters.AddWithValue("namespace",n);q.Parameters.AddWithValue("key",key);await q.ExecuteNonQueryAsync(token);}
        public async Task<bool> ExistsAsync(string n,string key,CancellationToken token){Validate(n,key);await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("SELECT 1 FROM state_values WHERE namespace=@namespace AND key=@key",c);q.Parameters.AddWithValue("namespace",n);q.Parameters.AddWithValue("key",key);return await q.ExecuteScalarAsync(token)!=null;}
        private static void Validate(string n,string key){if(string.IsNullOrEmpty(n))throw new ArgumentException("Required.",nameof(n));if(string.IsNullOrEmpty(key))throw new ArgumentException("Required.",nameof(key));}
    }
}
