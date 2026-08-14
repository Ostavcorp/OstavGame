using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ostav;

namespace Ostav.Persistence
{
    public enum OstavAccountStatus { Active, Disabled }
    public sealed class OstavAccount : IOstavIdentity
    {
        public OstavAccount(string id,string authSubject,string displayName,string preferredLocale,OstavAccountStatus status,DateTime createdUtc,DateTime updatedUtc){Id=id;AuthSubject=authSubject;DisplayName=displayName;PreferredLocale=preferredLocale;Status=status;CreatedUtc=createdUtc;UpdatedUtc=updatedUtc;}
        public string Id{get;private set;}public string IdentityType{get{return "account";}}public string AuthSubject{get;private set;}public string DisplayName{get;private set;}public string PreferredLocale{get;private set;}public OstavAccountStatus Status{get;private set;}public DateTime CreatedUtc{get;private set;}public DateTime UpdatedUtc{get;private set;}
    }
    public interface IOstavAccountRepository
    { Task<OstavAccount> FindBySubjectAsync(string subject,CancellationToken token);Task SaveAsync(OstavAccount account,CancellationToken token); }
    public sealed class InMemoryOstavAccountRepository : IOstavAccountRepository
    {
        private readonly Dictionary<string,OstavAccount> accounts=new Dictionary<string,OstavAccount>(StringComparer.Ordinal);
        public Task<OstavAccount> FindBySubjectAsync(string subject,CancellationToken token){token.ThrowIfCancellationRequested();accounts.TryGetValue(subject,out OstavAccount account);return Task.FromResult(account);}
        public Task SaveAsync(OstavAccount account,CancellationToken token){if(account==null)throw new ArgumentNullException("account");token.ThrowIfCancellationRequested();accounts[account.AuthSubject]=account;return Task.CompletedTask;}
    }
    public sealed class SqliteOstavAccountRepository : IOstavAccountRepository
    {
        private readonly OstavSqliteDatabase database;public SqliteOstavAccountRepository(OstavSqliteDatabase database){this.database=database??throw new ArgumentNullException("database");}
        public async Task<OstavAccount> FindBySubjectAsync(string subject,CancellationToken token){if(string.IsNullOrEmpty(subject))throw new ArgumentException("Required.","subject");await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="SELECT identity_id,auth_subject,display_name,preferred_locale,status,created_utc,updated_utc FROM accounts WHERE auth_subject=$subject";command.Parameters.AddWithValue("$subject",subject);await using var reader=await command.ExecuteReaderAsync(token);if(!await reader.ReadAsync(token))return null;return Read(reader);}
        public async Task SaveAsync(OstavAccount account,CancellationToken token){if(account==null)throw new ArgumentNullException("account");await using var connection=database.Open();await using var command=connection.CreateCommand();command.CommandText="INSERT INTO accounts(identity_id,auth_subject,display_name,preferred_locale,status,created_utc,updated_utc) VALUES($id,$subject,$name,$locale,$status,$created,$updated) ON CONFLICT(identity_id) DO UPDATE SET auth_subject=excluded.auth_subject,display_name=excluded.display_name,preferred_locale=excluded.preferred_locale,status=excluded.status,updated_utc=excluded.updated_utc";command.Parameters.AddWithValue("$id",account.Id);command.Parameters.AddWithValue("$subject",account.AuthSubject);command.Parameters.AddWithValue("$name",account.DisplayName);command.Parameters.AddWithValue("$locale",account.PreferredLocale);command.Parameters.AddWithValue("$status",account.Status.ToString());command.Parameters.AddWithValue("$created",account.CreatedUtc.ToString("O",CultureInfo.InvariantCulture));command.Parameters.AddWithValue("$updated",account.UpdatedUtc.ToString("O",CultureInfo.InvariantCulture));await command.ExecuteNonQueryAsync(token);}
        private static OstavAccount Read(Microsoft.Data.Sqlite.SqliteDataReader r){return new OstavAccount(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),(OstavAccountStatus)Enum.Parse(typeof(OstavAccountStatus),r.GetString(4)),DateTime.Parse(r.GetString(5),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind),DateTime.Parse(r.GetString(6),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind));}
    }
    public sealed class OstavAccountService
    {
        private readonly IOstavAccountRepository repository;private readonly IOstavClock clock;public OstavAccountService(IOstavAccountRepository repository,IOstavClock clock){this.repository=repository??throw new ArgumentNullException("repository");this.clock=clock??throw new ArgumentNullException("clock");}
        public async Task<OstavAccount> GetOrCreateAsync(string subject,string displayName,string locale,CancellationToken token){if(string.IsNullOrEmpty(subject))throw new ArgumentException("Required.","subject");OstavAccount account=await repository.FindBySubjectAsync(subject,token);if(account!=null)return account;DateTime now=clock.UtcNow;account=new OstavAccount(Guid.NewGuid().ToString("N"),subject,string.IsNullOrWhiteSpace(displayName)?"Developer":displayName,string.IsNullOrWhiteSpace(locale)?"en":locale,OstavAccountStatus.Active,now,now);await repository.SaveAsync(account,token);return account;}
    }
}
