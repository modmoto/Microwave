using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microwave.Queries
{
    public class VersionRepository : IVersionRepository
    {
        private readonly QueryStorageContext _subscriptionContext;

        public VersionRepository(QueryStorageContext subscriptionContext)
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
                .Upsert(new LastProcessedVersionDbo(version.EventType, version.LastVersion))
                .RunAsync();
        }
    }
}