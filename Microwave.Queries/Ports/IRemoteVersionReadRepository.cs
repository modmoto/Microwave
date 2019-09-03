using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IRemoteVersionReadRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
    }
}