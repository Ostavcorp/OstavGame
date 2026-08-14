using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavRule
    {
        string Id { get; }
        string EventType { get; }
        int Priority { get; }
        Task<bool> MatchesAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken);
        Task<IReadOnlyCollection<IOstavAction>> CreateActionsAsync(
            IOstavEvent eventData,
            CancellationToken cancellationToken);
    }
}
