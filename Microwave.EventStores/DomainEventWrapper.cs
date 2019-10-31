using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores
{
    public class DomainEventWrapper
    {
        public long OverallVersion { get; set; }
        public long EntityStreamVersion { get; set; }
        public string DomainEventType => DomainEvent.GetType().Name;
        public IDomainEvent DomainEvent { get; set; }
    }
}