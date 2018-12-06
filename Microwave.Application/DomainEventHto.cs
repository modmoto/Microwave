using Microwave.Domain;

namespace Microwave.Application
{
    public class DomainEventHto<TEvent> where TEvent : IDomainEvent
    {
        public long Created { get; set; }
        public long Version { get; set; }
        public TEvent DomainEvent { get; set; }
    }

    public class DomainEventWrapper
    {
        public long Created { get; set; }
        public long Version { get; set; }
        public IDomainEvent DomainEvent { get; set; }
    }
}