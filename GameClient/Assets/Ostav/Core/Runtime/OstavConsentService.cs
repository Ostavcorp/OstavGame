using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavConsentService : IOstavConsentService
    {
        private readonly IOstavConsentStore store;

        public OstavConsentService(IOstavConsentStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            this.store = store;
        }

        public Task<bool> HasConsentAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken)
        {
            return store.HasConsentAsync(identity, permission, cancellationToken);
        }
    }
}
