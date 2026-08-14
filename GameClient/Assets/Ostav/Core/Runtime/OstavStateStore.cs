using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavStateStore
    {
        Task<byte[]> GetAsync(string stateNamespace, string key, CancellationToken cancellationToken);
        Task SetAsync(string stateNamespace, string key, byte[] value, CancellationToken cancellationToken);
        Task RemoveAsync(string stateNamespace, string key, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string stateNamespace, string key, CancellationToken cancellationToken);
    }

    public sealed class InMemoryOstavStateStore : IOstavStateStore
    {
        private readonly Dictionary<string, byte[]> values = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        public Task<byte[]> GetAsync(string n, string key, CancellationToken token)
        { Validate(n,key); token.ThrowIfCancellationRequested(); values.TryGetValue(Key(n,key),out byte[] value); return Task.FromResult(value == null ? null : (byte[])value.Clone()); }
        public Task SetAsync(string n, string key, byte[] value, CancellationToken token)
        { Validate(n,key); if(value==null)throw new ArgumentNullException("value"); token.ThrowIfCancellationRequested(); values[Key(n,key)]=(byte[])value.Clone(); return Task.FromResult(0); }
        public Task RemoveAsync(string n, string key, CancellationToken token)
        { Validate(n,key); token.ThrowIfCancellationRequested(); values.Remove(Key(n,key)); return Task.FromResult(0); }
        public Task<bool> ExistsAsync(string n, string key, CancellationToken token)
        { Validate(n,key); token.ThrowIfCancellationRequested(); return Task.FromResult(values.ContainsKey(Key(n,key))); }
        private static string Key(string n,string key){return n+"\n"+key;}
        private static void Validate(string n,string key){if(string.IsNullOrEmpty(n))throw new ArgumentException("Required.","stateNamespace");if(string.IsNullOrEmpty(key))throw new ArgumentException("Required.","key");}
    }
}
