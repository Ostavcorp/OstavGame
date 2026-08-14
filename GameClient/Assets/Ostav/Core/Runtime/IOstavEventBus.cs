using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavEventBus
    {
        void Subscribe(IOstavEventHandler handler);
        void Unsubscribe(IOstavEventHandler handler);
        Task PublishAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken);
    }
}
