using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreContext : DbContext
    {
        public DbSet<DomainEventWrapper> DomainEvents { get; set; }
        public DbSet<EntityStream> EntityStreams { get; set; }
        public DbSet<TypeStream> TypeStreams { get; set; }

        public EventStoreContext(DbContextOptions<EventStoreContext> options) :
            base(options)
        {
        }
    }

    public class DomainEventWrapper
    {
        public Guid Id { get; set; }
        public DomainEventDbo DomainEvent { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
    }

    public class DomainEventDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck]
        public long Created { get; set; }
    }

    public class EntityStream
    {
        [Key]
        public Guid EntityId { get; set; }
        public ICollection<DomainEventWrapper> DomainEvents { get; set; }
        public long Version { get; set; }
    }

    public class TypeStream
    {
        [Key]
        public string DomainEventType { get; set; }
        public ICollection<DomainEventWrapper> DomainEvents { get; set; }
        public long Version { get; set; }
    }
}