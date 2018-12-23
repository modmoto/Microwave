using Microwave.Domain;

namespace Microwave.Application
{
    public class DomainEventWrapper
    {
        public long Created { get; set; }
        public long Version { get; set; }
        public string DomainEventType => DomainEvent.GetType().Name;
        public IDomainEvent DomainEvent { get; set; }
    }
}