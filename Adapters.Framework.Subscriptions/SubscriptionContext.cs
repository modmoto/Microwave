using Microsoft.EntityFrameworkCore;

namespace Microwave.Subscriptions
{
    public sealed class SubscriptionContext : DbContext
    {
        public DbSet<LastProcessedVersionDbo> ProcessedVersions { get; set; }

        public SubscriptionContext(DbContextOptions<SubscriptionContext> options) :
            base(options)
        {
        }
    }
}