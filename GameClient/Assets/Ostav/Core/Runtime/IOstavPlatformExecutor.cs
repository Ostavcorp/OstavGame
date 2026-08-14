using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavPlatformExecutor
    {
        Task<IOstavActionResult> ExecuteIntentAsync(
            IOstavIntent intent,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
