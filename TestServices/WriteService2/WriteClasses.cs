using Microwave.Domain;

namespace WriteService2
{
    public class Entity2 : Entity, IApply<Event3>, IApply<Event4>
    {
        public void Apply(Event3 domainEvent)
        {
        }

        public void Apply(Event4 domainEvent)
        {
        }
    }
    public class Event3 : IDomainEvent
    {
        public Event3(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class Event4 : IDomainEvent
    {
        public Event4(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}