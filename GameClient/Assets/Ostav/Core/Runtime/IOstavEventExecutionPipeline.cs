using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavEventExecutionPipeline
    {
        Task<IReadOnlyCollection<IOstavActionResult>> ExecuteAsync(
            IOstavEvent eventData,
            IOstavExecutionContext context,
            CancellationToken cancellationToken);
    }
}
