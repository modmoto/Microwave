using System;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class EventStoreContext : DbContext
    {
        public DbSet<DomainEventDbo> DomainEvents { get; set; }


        public EventStoreContext(DbContextOptions<EventStoreContext> options) :
            base(options)
        {
        }
    }

    public class DomainEventDbo
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Payload { get; set; }
    }
}