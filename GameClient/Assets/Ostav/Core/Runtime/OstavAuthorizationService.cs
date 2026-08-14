using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavAuthorizationService : IOstavAuthorizationService
    {
        private readonly IOstavConsentService consentService;

        public OstavAuthorizationService(IOstavConsentService consentService)
        {
            if (consentService == null)
            {
                throw new ArgumentNullException("consentService");
            }

            this.consentService = consentService;
        }

        public Task<bool> IsAuthorizedAsync(
            IOstavExecutionContext context,
            IOstavPermission permission,
            CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.Identity == null)
            {
                throw new ArgumentNullException("context.Identity");
            }

            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return consentService.HasConsentAsync(
                context.Identity,
                permission,
                cancellationToken);
        }
    }
}
