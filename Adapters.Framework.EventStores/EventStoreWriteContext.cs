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

    public class DomainEventDboCopy
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck]
        public long Created { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
    }

    public class DomainEventDboCopyOverallStream
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

    public class TypeStream
    {
        [Key]
        public string DomainEventType { get; set; }
        public ICollection<DomainEventDboCopy> DomainEvents { get; set; }
        public long Version { get; set; }
    }

    public sealed class EventStoreReadContext : DbContext
    {
        public DbSet<TypeStream> TypeStreams { get; set; }
        public DbSet<DomainEventDboCopyOverallStream> OverallStream { get; set; }

        public EventStoreReadContext(DbContextOptions<EventStoreReadContext> options) :
            base(options)
        {
        }
    }
}