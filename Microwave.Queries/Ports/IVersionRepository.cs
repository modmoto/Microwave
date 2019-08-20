using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IVersionRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
        Task<DateTimeOffset> GetRemoteVersionAsync(string domainEventType);
        Task SaveVersionAsync(LastProcessedVersion version);
        Task SaveRemoteVersionAsync(LastProcessedVersion version);
    }

    public class LastProcessedVersion
    {
        public LastProcessedVersion(string eventType, DateTimeOffset lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        public string EventType { get; set; }
        public DateTimeOffset LastVersion { get; set; }
    }
}