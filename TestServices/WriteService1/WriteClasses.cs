using Microwave.Domain;

namespace WriteService1
{
    public class Entity1 : Entity, IApply<Event1>, IApply<Event2>
    {
        public void Apply(Event1 event1)
        {
        }

        public void Apply(Event2 event2)
        {
        }
    }

    public class Event1 : IDomainEvent
    {
        public Event1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class Event2 : IDomainEvent
    {
        public Event2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}