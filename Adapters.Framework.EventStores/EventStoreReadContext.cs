using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreReadContext : DbContext
    {
        public DbSet<TypeStream> TypeStreams { get; set; }
        public DbSet<DomainEventDboCopyOverallStream> OverallStream { get; set; }

        public EventStoreReadContext(DbContextOptions<EventStoreReadContext> options) :
            base(options)
        {
        }
    }

    public class TypeStream
    {
        [Key] public string DomainEventType { get; set; }
        public ICollection<DomainEventDboCopy> DomainEvents { get; set; }
        public long Version { get; set; }
    }

    public class DomainEventDboCopy
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck] public long Created { get; set; }
        [ConcurrencyCheck] public long Version { get; set; }
    }

    public class DomainEventDboCopyOverallStream
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [ConcurrencyCheck] public long Created { get; set; }
        [ConcurrencyCheck] public long Version { get; set; }
    }
}