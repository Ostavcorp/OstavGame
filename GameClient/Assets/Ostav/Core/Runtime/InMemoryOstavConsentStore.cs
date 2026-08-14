using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class InMemoryOstavConsentStore : IOstavConsentStore
    {
        private readonly List<Grant> grants = new List<Grant>();

        public Task<bool> HasConsentAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken)
        {
            ValidateArguments(identity, permission);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(FindGrant(identity.Id, permission.Id) >= 0);
        }

        public Task GrantAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken)
        {
            ValidateArguments(identity, permission);
            cancellationToken.ThrowIfCancellationRequested();

            if (FindGrant(identity.Id, permission.Id) < 0)
            {
                grants.Add(new Grant(identity.Id, permission.Id));
            }

            return Task.FromResult(0);
        }

        public Task RevokeAsync(
            IOstavIdentity identity,
            IOstavPermission permission,
            CancellationToken cancellationToken)
        {
            ValidateArguments(identity, permission);
            cancellationToken.ThrowIfCancellationRequested();

            int index = FindGrant(identity.Id, permission.Id);
            if (index >= 0)
            {
                grants.RemoveAt(index);
            }

            return Task.FromResult(0);
        }

        private int FindGrant(string identityId, string permissionId)
        {
            for (int index = 0; index < grants.Count; index++)
            {
                Grant grant = grants[index];
                if (string.Equals(grant.IdentityId, identityId, StringComparison.Ordinal) &&
                    string.Equals(grant.PermissionId, permissionId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static void ValidateArguments(
            IOstavIdentity identity,
            IOstavPermission permission)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
        }

        private sealed class Grant
        {
            public Grant(string identityId, string permissionId)
            {
                IdentityId = identityId;
                PermissionId = permissionId;
            }

            public string IdentityId { get; private set; }
            public string PermissionId { get; private set; }
        }
    }
}
