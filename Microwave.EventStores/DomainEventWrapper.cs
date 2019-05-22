using System;
using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores
{
    public class DomainEventWrapper
    {
        public DateTimeOffset Created { get; set; }
        public long Version { get; set; }
        public string DomainEventType => DomainEvent.GetType().Name;
        public IDomainEvent DomainEvent { get; set; }
    }
}