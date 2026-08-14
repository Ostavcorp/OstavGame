using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavEventExecutionPipeline : IOstavEventExecutionPipeline
    {
        private readonly IOstavRuleEngine ruleEngine;
        private readonly IOstavActionDispatcher actionDispatcher;
        private readonly IOstavAuditSink auditSink;
        private readonly IOstavClock clock;

        public OstavEventExecutionPipeline(
            IOstavRuleEngine ruleEngine,
            IOstavActionDispatcher actionDispatcher)
            : this(ruleEngine, actionDispatcher, null, null)
        {
        }

        public OstavEventExecutionPipeline(
            IOstavRuleEngine ruleEngine,
            IOstavActionDispatcher actionDispatcher,
            IOstavAuditSink auditSink,
            IOstavClock clock)
        {
            if (ruleEngine == null)
            {
                throw new ArgumentNullException("ruleEngine");
            }

            if (actionDispatcher == null)
            {
                throw new ArgumentNullException("actionDispatcher");
            }

            this.ruleEngine = ruleEngine;
            this.actionDispatcher = actionDispatcher;
            this.auditSink = auditSink;
            this.clock = clock;
        }

        public async Task<IReadOnlyCollection<IOstavActionResult>> ExecuteAsync(
            IOstavEvent eventData,
            IOstavExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyCollection<IOstavAction> actions =
                await ruleEngine.EvaluateAsync(eventData, cancellationToken);
            var results = new List<IOstavActionResult>();

            if (actions == null)
            {
                return results.AsReadOnly();
            }

            foreach (IOstavAction action in actions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IOstavActionResult result =
                    await actionDispatcher.DispatchAsync(action, context, cancellationToken);
                IOstavExecutionMetadata resultMetadata =
                    (context as IOstavExecutionMetadataContext)?.Metadata;
                if (resultMetadata != null && result is OstavActionResult concrete &&
                    concrete.Metadata == null)
                {
                    result = new OstavActionResult(concrete.Success, concrete.Code,
                        concrete.Message, concrete.Payload, resultMetadata);
                }
                results.Add(result);
                if (auditSink != null)
                {
                    IOstavExecutionMetadata metadata = resultMetadata;
                    try
                    {
                        await auditSink.WriteAsync(new OstavAuditRecord(
                            metadata?.RequestId,
                            metadata?.CorrelationId,
                            context.Identity?.Id,
                            eventData.SourceModuleId,
                            action.TargetCapabilityId,
                            action.ActionType,
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
            }

            return results.AsReadOnly();
        }
    }
}
