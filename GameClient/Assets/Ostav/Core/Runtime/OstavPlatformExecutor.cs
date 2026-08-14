using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavPlatformExecutor : IOstavPlatformExecutor
    {
        private readonly IOstavPlatformRuntime runtime;
        private readonly IOstavAuditSink auditSink;
        private readonly IOstavClock clock;
        private readonly IOstavIdempotencyStore idempotencyStore;

        public OstavPlatformExecutor(IOstavPlatformRuntime runtime)
            : this(runtime, null, null, null)
        {
        }

        public OstavPlatformExecutor(
            IOstavPlatformRuntime runtime,
            IOstavAuditSink auditSink,
            IOstavClock clock,
            IOstavIdempotencyStore idempotencyStore)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException("runtime");
            }

            this.runtime = runtime;
            this.auditSink = auditSink;
            this.clock = clock;
            this.idempotencyStore = idempotencyStore;
        }

        public async Task<IOstavActionResult> ExecuteIntentAsync(
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

            IOstavExecutionMetadata metadata =
                (context as IOstavExecutionMetadataContext)?.Metadata;
            IOstavConcurrentIdempotencyStore concurrentStore =
                idempotencyStore as IOstavConcurrentIdempotencyStore;
            if (concurrentStore != null && metadata != null)
            {
                IOstavIdempotencyLease lease = await concurrentStore.AcquireAsync(
                    metadata.RequestId, cancellationToken);
                try
                {
                    return await ExecuteAfterValidationAsync(intent, context,
                        metadata, cancellationToken);
                }
                finally
                {
                    await lease.ReleaseAsync(CancellationToken.None);
                }
            }

            return await ExecuteAfterValidationAsync(intent, context,
                metadata, cancellationToken);
        }

        private async Task<IOstavActionResult> ExecuteAfterValidationAsync(
            IOstavIntent intent,
            IOstavExecutionContext context,
            IOstavExecutionMetadata metadata,
            CancellationToken cancellationToken)
        {
            if (idempotencyStore != null && metadata != null)
            {
                IOstavActionResult previous = await idempotencyStore.GetAsync(
                    metadata.RequestId,
                    cancellationToken);
                if (previous != null)
                {
                    return previous;
                }
            }

            IReadOnlyCollection<IOstavModuleProvider> providers =
                runtime.ModuleCatalog.FindByCapability(intent.TargetCapabilityId);
            IOstavModuleProvider provider = null;
            foreach (IOstavModuleProvider candidate in providers)
            {
                provider = candidate;
                break;
            }

            if (provider == null)
            {
                return await CompleteAsync(new OstavActionResult(
                    false,
                    OstavPlatformCodes.CapabilityNotAvailable,
                    "The capability is not available."), intent, context, null,
                    metadata, cancellationToken);
            }

            IOstavModuleHealth health =
                (provider as IOstavModuleHealthProvider)?.Health;
            if (health != null &&
                (health.State == OstavModuleState.Unavailable ||
                 health.State == OstavModuleState.Stopped ||
                 !health.IsCapabilityAvailable(intent.TargetCapabilityId)))
            {
                return await CompleteAsync(new OstavActionResult(
                    false, OstavPlatformCodes.ModuleUnavailable,
                    "The module is unavailable."), intent, context,
                    provider, metadata, cancellationToken);
            }

            IOstavModuleManifest manifest = provider.Manifest;
            if (manifest != null && manifest.RequiredPermissions != null)
            {
                foreach (IOstavPermission permission in manifest.RequiredPermissions)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!await runtime.Authorization.IsAuthorizedAsync(
                        context,
                        permission,
                        cancellationToken))
                    {
                        return await CompleteAsync(new OstavActionResult(
                            false,
                            OstavPlatformCodes.PermissionDenied,
                            "A required permission was denied."), intent, context,
                            provider, metadata, cancellationToken);
                    }
                }
            }

            IOstavActionResult result = await runtime.Intents.RouteAsync(
                intent,
                context,
                cancellationToken);
            if (metadata != null && result is OstavActionResult concrete && concrete.Metadata == null)
            {
                result = new OstavActionResult(concrete.Success, concrete.Code,
                    concrete.Message, concrete.Payload, metadata);
            }
            return await CompleteAsync(result, intent, context, provider,
                metadata, cancellationToken);
        }

        private async Task<IOstavActionResult> CompleteAsync(
            IOstavActionResult result, IOstavIntent intent,
            IOstavExecutionContext context, IOstavModuleProvider provider,
            IOstavExecutionMetadata metadata, CancellationToken cancellationToken)
        {
            if (idempotencyStore != null && metadata != null)
            {
                await idempotencyStore.StoreAsync(
                    metadata.RequestId, result, cancellationToken);
            }

            if (auditSink != null)
            {
                try
                {
                    await auditSink.WriteAsync(new OstavAuditRecord(
                        metadata?.RequestId,
                        metadata?.CorrelationId,
                        context.Identity?.Id,
                        provider?.Module?.Id,
                        intent.TargetCapabilityId,
                        intent.IntentType,
                        result.Code,
                        clock == null ? DateTime.UtcNow : clock.UtcNow,
                        result.Success), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                }
            }

            return result;
        }
    }
}
