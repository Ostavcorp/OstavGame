using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavActionDispatcher : IOstavActionDispatcher
    {
        private readonly List<IOstavActionHandler> handlers =
            new List<IOstavActionHandler>();
        private readonly IOstavAuthorizationService authorization;

        public OstavActionDispatcher()
        {
        }

        public OstavActionDispatcher(IOstavAuthorizationService authorization)
        {
            this.authorization = authorization ??
                throw new ArgumentNullException("authorization");
        }

        public void Register(IOstavActionHandler handler)
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

        public void Unregister(IOstavActionHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            handlers.Remove(handler);
        }

        public async Task<IOstavActionResult> DispatchAsync(
            IOstavAction action,
            IOstavExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            cancellationToken.ThrowIfCancellationRequested();

            IOstavSecuredAction securedAction = action as IOstavSecuredAction;
            if (securedAction != null && securedAction.RequiredPermissions != null)
            {
                foreach (IOstavPermission permission in securedAction.RequiredPermissions)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (authorization == null || !await authorization.IsAuthorizedAsync(
                        context, permission, cancellationToken))
                    {
                        return new OstavActionResult(false,
                            OstavPlatformCodes.PermissionDenied,
                            "A required permission was denied.");
                    }
                }
            }

            foreach (IOstavActionHandler handler in handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.Equals(
                        handler.CapabilityId,
                        action.TargetCapabilityId,
                        StringComparison.Ordinal) &&
                    handler.CanHandle(action))
                {
                    return await handler.HandleAsync(action, context, cancellationToken);
                }
            }

            return new OstavActionResult(
                false,
                OstavPlatformCodes.NoHandler,
                "No handler is available for the action.");
        }
    }
}
