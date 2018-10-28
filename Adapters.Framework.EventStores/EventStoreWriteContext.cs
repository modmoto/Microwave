using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreWriteContext : DbContext
    {
        public DbSet<DomainEventDbo> EntityStreams { get; set; }

        public EventStoreWriteContext(DbContextOptions<EventStoreWriteContext> options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DomainEventDbo>()
                .HasKey(p => new {p.EntityId , p.Version});
        }
    }

    public class DomainEventDbo
    {
        public string EntityId { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck]
        public long Created { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
    }
}