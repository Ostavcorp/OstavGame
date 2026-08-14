using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public sealed class OstavRuleEngine : IOstavRuleEngine
    {
        private readonly List<IOstavRule> rules = new List<IOstavRule>();
        private readonly HashSet<string> ruleIds =
            new HashSet<string>(StringComparer.Ordinal);

        public void Register(IOstavRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            if (!ruleIds.Add(rule.Id))
            {
                throw new InvalidOperationException(
                    "A rule with Id '" + rule.Id + "' is already registered.");
            }

            rules.Add(rule);
        }

        public void Unregister(IOstavRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            if (rules.Remove(rule))
            {
                ruleIds.Remove(rule.Id);
            }
        }

        public async Task<IReadOnlyCollection<IOstavAction>> EvaluateAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            cancellationToken.ThrowIfCancellationRequested();

            List<IOstavRule> matchingRules = GetMatchingRules(eventData.EventType);
            var actions = new List<IOstavAction>();

            foreach (IOstavRule rule in matchingRules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await rule.MatchesAsync(eventData, cancellationToken))
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();
                IReadOnlyCollection<IOstavAction> createdActions =
                    await rule.CreateActionsAsync(eventData, cancellationToken);

                if (createdActions == null)
                {
                    continue;
                }

                foreach (IOstavAction action in createdActions)
                {
                    if (action != null)
                    {
                        actions.Add(action);
                    }
                }
            }

            return actions.AsReadOnly();
        }

        private List<IOstavRule> GetMatchingRules(string eventType)
        {
            var matchingRules = new List<IOstavRule>();

            foreach (IOstavRule rule in rules)
            {
                if (!string.Equals(rule.EventType, eventType, StringComparison.Ordinal))
                {
                    continue;
                }

                int insertionIndex = matchingRules.Count;
                while (insertionIndex > 0 &&
                    matchingRules[insertionIndex - 1].Priority < rule.Priority)
                {
                    insertionIndex--;
                }

                matchingRules.Insert(insertionIndex, rule);
            }

            return matchingRules;
        }
    }
}
