using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Yadl.Abstractions
{
    public interface ISqlServerBulk
    {
        Task ExecuteAsync(ICollection<YadlMessage> messages, CancellationToken cancellationToken);
    }
}