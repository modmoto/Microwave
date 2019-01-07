using System;
using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IVersionRepository
    {
        Task<DateTimeOffset> GetVersionAsync(string domainEventType);
        Task SaveVersion(LastProcessedVersion version);
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