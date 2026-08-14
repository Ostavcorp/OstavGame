using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavEventHandler
    {
        string EventType { get; }
        Task HandleAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken);
    }
}
