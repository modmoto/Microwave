using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IRemoteVersionReadModelRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
    }
}