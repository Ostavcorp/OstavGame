using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavConsentService
    {
        Task<bool> HasConsentAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken);
    }
}
