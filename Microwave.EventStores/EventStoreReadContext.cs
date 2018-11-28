using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Microwave.EventStores
{
    public sealed class EventStoreReadContext : DbContext
    {
        public DbSet<DomainEventTypeDbo> TypeStreams { get; set; }

        public EventStoreReadContext(DbContextOptions<EventStoreReadContext> options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DomainEventTypeDbo>()
                .HasKey(p => new {p.DomainEventType , p.Version});
        }
    }

    public class DomainEventTypeDbo
    {
        public string DomainEventType { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck]
        public long Created { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
    }
}