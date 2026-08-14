using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavActionDispatcher
    {
        void Register(IOstavActionHandler handler);
        void Unregister(IOstavActionHandler handler);
        Task<IOstavActionResult> DispatchAsync(
            IOstavAction action,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
