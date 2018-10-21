using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Framework
{
    public class VersionRepository : IVersionRepository
    {
        private readonly SubscriptionContext _subscriptionContext;

        public VersionRepository(SubscriptionContext subscriptionContext)
        {
            _subscriptionContext = subscriptionContext;
        }

        public async Task<long> GetVersionAsync(string domainEventType)
        {
            var lastProcessedVersion = await _subscriptionContext.ProcessedVersions.FirstOrDefaultAsync(version => version.EventType == domainEventType);
            if (lastProcessedVersion == null) return -1L;
            return lastProcessedVersion.LastVersion;
        }

        public async Task SaveVersion(LastProcessedVersion version)
        {
            var lastProcessedVersionDbo = _subscriptionContext.ProcessedVersions.FirstOrDefault(ev => ev.EventType == version.EventType);
            if (lastProcessedVersionDbo == null)
            {
                _subscriptionContext.ProcessedVersions.Add(new LastProcessedVersionDbo(version.EventType, version.LastVersion));
            }
            else
            {
                _subscriptionContext.Update(lastProcessedVersionDbo);
            }
            await _subscriptionContext.SaveChangesAsync();
        }
    }

    public class LastProcessedVersionDbo
    {
        public LastProcessedVersionDbo(string eventType, long lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        [Key]
        public string EventType { get; set; }
        public long LastVersion { get; set; }
    }
}