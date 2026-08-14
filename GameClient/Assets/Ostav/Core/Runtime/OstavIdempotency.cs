using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavIdempotencyStore
    {
        Task<IOstavActionResult> GetAsync(string requestId, CancellationToken cancellationToken);
        Task StoreAsync(string requestId, IOstavActionResult result, CancellationToken cancellationToken);
    }

    public interface IOstavIdempotencyLease
    {
        Task ReleaseAsync(CancellationToken cancellationToken);
    }

    public interface IOstavConcurrentIdempotencyStore : IOstavIdempotencyStore
    {
        Task<IOstavIdempotencyLease> AcquireAsync(
            string requestId,
            CancellationToken cancellationToken);
    }

    public sealed class InMemoryOstavIdempotencyStore : IOstavIdempotencyStore
    {
        private readonly Dictionary<string, IOstavActionResult> results =
            new Dictionary<string, IOstavActionResult>(StringComparer.Ordinal);
        public Task<IOstavActionResult> GetAsync(string requestId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("A value is required.", "requestId");
            cancellationToken.ThrowIfCancellationRequested();
            results.TryGetValue(requestId, out IOstavActionResult result);
            return Task.FromResult(result);
        }
        public Task StoreAsync(string requestId, IOstavActionResult result, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("A value is required.", "requestId");
            if (result == null) throw new ArgumentNullException("result");
            cancellationToken.ThrowIfCancellationRequested(); results[requestId] = result;
            return Task.FromResult(0);
        }
    }
}
