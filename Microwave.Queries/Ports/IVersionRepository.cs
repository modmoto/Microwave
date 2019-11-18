using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IVersionRepository
    {
        Task<long> GetVersionAsync(string domainEventType);
        Task SaveVersion(LastProcessedVersion version);
    }

    public class LastProcessedVersion
    {
        public LastProcessedVersion(string eventType, long lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        public string EventType { get; }
        public long LastVersion { get; }
    }
}