using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.Attributes;

namespace Microwave.EventStores
{
    public sealed class EventStoreContext : DbContext
    {
        public DbSet<DomainEventDbo> EntityStreams { get; set; }
        public DbSet<SnapShotDbo> SnapShots { get; set; }

        public EventStoreContext(DbContextOptions<EventStoreContext> options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SnapShotDbo>()
                .HasKey(p => new {p.EntityId});
        }
    }

    public class SnapShotDbo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string EntityId { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
    }

    public class DomainEventDbo
    {
        public DomainEventKey Key { get; set; }

        [BsonId]
        public string KeyHack {
            get => $"{Key.EntityId}_{Key.Version}";
            set => Key = new DomainEventKey
            {
                EntityId = value.Split('_')[0],
                Version = long.Parse(value.Split('_')[1])
            };
        }
        public string Payload { get; set; }
        public long Created { get; set; }
        public string EventType { get; set; }
    }

    public class DomainEventKey
    {
        public string EntityId { get; set; }
        public long Version { get; set; }
    }
}