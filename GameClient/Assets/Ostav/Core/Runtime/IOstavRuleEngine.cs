using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavRuleEngine
    {
        void Register(IOstavRule rule);
        void Unregister(IOstavRule rule);
        Task<IReadOnlyCollection<IOstavAction>> EvaluateAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken);
    }
}
