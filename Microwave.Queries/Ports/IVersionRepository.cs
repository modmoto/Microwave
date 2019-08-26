using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IVersionRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
        Task SaveVersionAsync(LastProcessedVersion version);
    }
}