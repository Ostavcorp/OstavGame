using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavAuthorizationService
    {
        Task<bool> IsAuthorizedAsync(
            IOstavExecutionContext context,
            IOstavPermission permission,
            CancellationToken cancellationToken);
    }
}
