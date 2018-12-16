using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microwave.Queries
{
    public class VersionRepository : IVersionRepository
    {
        private readonly ReadModelStorageContext _subscriptionContext;

        public VersionRepository(ReadModelStorageContext subscriptionContext)
        {
            _subscriptionContext = subscriptionContext;
        }

        public async Task<long> GetVersionAsync(string domainEventType)
        {
            var lastProcessedVersion =
                await _subscriptionContext.ProcessedVersions.FirstOrDefaultAsync(version =>
                    version.EventType == domainEventType);
            if (lastProcessedVersion == null) return 0L;
            return lastProcessedVersion.LastVersion;
        }

        public async Task SaveVersion(LastProcessedVersion version)
        {
            await _subscriptionContext.ProcessedVersions
                .Upsert(new LastProcessedVersionDbo {
                    EventType = version.EventType,
                    LastVersion = version.LastVersion})
                .RunAsync();
        }
    }
}