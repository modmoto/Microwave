using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreWriteContext : DbContext
    {
        public DbSet<EntityStream> EntityStreams { get; set; }

        public EventStoreWriteContext(DbContextOptions<EventStoreWriteContext> options) :
            base(options)
        {
        }
    }

    public class DomainEventDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck]
        public long Created { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
    }

    public class EntityStream
    {
        [Key]
        public Guid EntityId { get; set; }
        public ICollection<DomainEventDbo> DomainEvents { get; set; }
        public long Version { get; set; }
    }
}