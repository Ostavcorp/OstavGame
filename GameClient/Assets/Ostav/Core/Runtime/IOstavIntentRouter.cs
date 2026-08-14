using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavIntentRouter
    {
        Task<IOstavActionResult> RouteAsync(
            IOstavIntent intent,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
