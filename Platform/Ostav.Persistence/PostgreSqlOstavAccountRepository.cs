using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Ostav.Persistence
{
    public sealed class PostgreSqlOstavAccountRepository : IOstavAccountRepository
    {
        private readonly OstavPostgreSqlDatabase database;public PostgreSqlOstavAccountRepository(OstavPostgreSqlDatabase database){this.database=database??throw new ArgumentNullException(nameof(database));}
        public async Task<OstavAccount> FindBySubjectAsync(string subject,CancellationToken token){if(string.IsNullOrEmpty(subject))throw new ArgumentException("Required.",nameof(subject));await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("SELECT identity_id,auth_subject,display_name,preferred_locale,status,created_utc,updated_utc FROM accounts WHERE auth_subject=@subject",c);q.Parameters.AddWithValue("subject",subject);await using NpgsqlDataReader r=await q.ExecuteReaderAsync(token);if(!await r.ReadAsync(token))return null;return new OstavAccount(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),(OstavAccountStatus)Enum.Parse(typeof(OstavAccountStatus),r.GetString(4)),r.GetDateTime(5).ToUniversalTime(),r.GetDateTime(6).ToUniversalTime());}
        public async Task SaveAsync(OstavAccount account,CancellationToken token){if(account==null)throw new ArgumentNullException(nameof(account));await using NpgsqlConnection c=await database.OpenAsync(token);await using NpgsqlCommand q=new NpgsqlCommand("INSERT INTO accounts(identity_id,auth_subject,display_name,preferred_locale,status,created_utc,updated_utc) VALUES(@id,@subject,@name,@locale,@status,@created,@updated) ON CONFLICT(identity_id) DO UPDATE SET auth_subject=excluded.auth_subject,display_name=excluded.display_name,preferred_locale=excluded.preferred_locale,status=excluded.status,updated_utc=excluded.updated_utc",c);q.Parameters.AddWithValue("id",account.Id);q.Parameters.AddWithValue("subject",account.AuthSubject);q.Parameters.AddWithValue("name",account.DisplayName);q.Parameters.AddWithValue("locale",account.PreferredLocale);q.Parameters.AddWithValue("status",account.Status.ToString());q.Parameters.AddWithValue("created",account.CreatedUtc);q.Parameters.AddWithValue("updated",account.UpdatedUtc);await q.ExecuteNonQueryAsync(token);}
    }
}
