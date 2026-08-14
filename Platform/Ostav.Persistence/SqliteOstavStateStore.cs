using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Ostav;

namespace Ostav.Persistence
{
    public sealed class SqliteOstavStateStore : IOstavStateStore
    {
        private readonly OstavSqliteDatabase database;public SqliteOstavStateStore(OstavSqliteDatabase database){this.database=database??throw new ArgumentNullException("database");}
        public async Task<byte[]> GetAsync(string n,string key,CancellationToken token){Validate(n,key);await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="SELECT value FROM state_values WHERE namespace=$n AND key=$k";command.Parameters.AddWithValue("$n",n);command.Parameters.AddWithValue("$k",key);object value=await command.ExecuteScalarAsync(token);return value==null?null:(byte[])value;}
        public async Task SetAsync(string n,string key,byte[] value,CancellationToken token){Validate(n,key);if(value==null)throw new ArgumentNullException("value");await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="INSERT INTO state_values(namespace,key,value) VALUES($n,$k,$v) ON CONFLICT(namespace,key) DO UPDATE SET value=excluded.value";command.Parameters.AddWithValue("$n",n);command.Parameters.AddWithValue("$k",key);command.Parameters.Add("$v",SqliteType.Blob).Value=value;await command.ExecuteNonQueryAsync(token);}
        public async Task RemoveAsync(string n,string key,CancellationToken token){Validate(n,key);await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="DELETE FROM state_values WHERE namespace=$n AND key=$k";command.Parameters.AddWithValue("$n",n);command.Parameters.AddWithValue("$k",key);await command.ExecuteNonQueryAsync(token);}
        public async Task<bool> ExistsAsync(string n,string key,CancellationToken token){Validate(n,key);await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="SELECT 1 FROM state_values WHERE namespace=$n AND key=$k";command.Parameters.AddWithValue("$n",n);command.Parameters.AddWithValue("$k",key);return await command.ExecuteScalarAsync(token)!=null;}
        private static void Validate(string n,string key){if(string.IsNullOrEmpty(n))throw new ArgumentException("Required.","stateNamespace");if(string.IsNullOrEmpty(key))throw new ArgumentException("Required.","key");}
    }
}
