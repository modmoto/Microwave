using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IRemoteVersionRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
        Task SaveVersionAsync(LastProcessedVersion version);
    }
}