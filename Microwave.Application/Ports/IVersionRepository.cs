using System.Threading.Tasks;

namespace Microwave.Application.Ports
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

        public string EventType { get; set; }
        public long LastVersion { get; set; }
    }
}