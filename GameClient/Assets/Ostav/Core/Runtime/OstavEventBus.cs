using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavEventBus : IOstavEventBus
    {
        private readonly List<IOstavEventHandler> handlers =
            new List<IOstavEventHandler>();

        public void Subscribe(IOstavEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (handlers.Contains(handler))
            {
                throw new InvalidOperationException("The handler is already subscribed.");
            }

            handlers.Add(handler);
        }

        public void Unsubscribe(IOstavEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            handlers.Remove(handler);
        }

        public async Task PublishAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            IOstavEventHandler[] subscribedHandlers = handlers.ToArray();

            foreach (IOstavEventHandler handler in subscribedHandlers)
            {
                if (string.Equals(
                    handler.EventType,
                    eventData.EventType,
                    StringComparison.Ordinal))
                {
                    await handler.HandleAsync(eventData, cancellationToken);
                }
            }
        }
    }
}
