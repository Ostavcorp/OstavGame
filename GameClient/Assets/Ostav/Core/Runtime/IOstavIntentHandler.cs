using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavIntentHandler
    {
        string CapabilityId { get; }
        bool CanHandle(IOstavIntent intent);
        Task<IOstavActionResult> HandleAsync(
            IOstavIntent intent,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
