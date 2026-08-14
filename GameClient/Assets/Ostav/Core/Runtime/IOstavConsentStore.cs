using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavConsentStore
    {
        Task<bool> HasConsentAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken);
        Task GrantAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken);
        Task RevokeAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken);
    }
}
