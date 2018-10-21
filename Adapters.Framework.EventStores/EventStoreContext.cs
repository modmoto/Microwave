using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreContext : DbContext
    {
        public DbSet<EntityStream> EntityStreams { get; set; }
        public DbSet<TypeStream> TypeStreams { get; set; }

        public EventStoreContext(DbContextOptions<EventStoreContext> options) :
            base(options)
        {
        }
    }

    public class DomainEventDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
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
        public ICollection<DomainEventDbo> DomainEvents { get; set; }
        public long Version { get; set; }
    }
}