using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavActionHandler
    {
        string CapabilityId { get; }
        bool CanHandle(IOstavAction action);
        Task<IOstavActionResult> HandleAsync(
            IOstavAction action,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
