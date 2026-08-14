using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavIntentRouter : IOstavIntentRouter
    {
        private readonly List<IOstavIntentHandler> handlers =
            new List<IOstavIntentHandler>();

        public void Register(IOstavIntentHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (handlers.Contains(handler))
            {
                throw new InvalidOperationException("The handler is already registered.");
            }

            handlers.Add(handler);
        }

        public void Unregister(IOstavIntentHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            handlers.Remove(handler);
        }

        public async Task<IOstavActionResult> RouteAsync(
            IOstavIntent intent,
            IOstavExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (intent == null)
            {
                throw new ArgumentNullException("intent");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(intent.TargetCapabilityId))
            {
                throw new ArgumentException(
                    "TargetCapabilityId is required.",
                    "intent");
            }

            cancellationToken.ThrowIfCancellationRequested();

            bool capabilityHandlerExists = false;

            foreach (IOstavIntentHandler handler in handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.Equals(
                    handler.CapabilityId,
                    intent.TargetCapabilityId,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                capabilityHandlerExists = true;

                if (handler.CanHandle(intent))
                {
                    return await handler.HandleAsync(intent, context, cancellationToken);
                }
            }

            return new OstavActionResult(
                false,
                capabilityHandlerExists
                    ? "INTENT_NOT_SUPPORTED"
                    : "CAPABILITY_NOT_AVAILABLE",
                capabilityHandlerExists
                    ? "The intent is not supported by the capability."
                    : "The capability is not available.");
        }
    }
}
