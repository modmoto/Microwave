using Microwave.Domain.EventSourcing;

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
        public Event1(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }


    public class Event2 : IDomainEvent
    {
        public Event2(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }

    public class EventMitSuperLangemNamen : IDomainEvent
    {
        public EventMitSuperLangemNamen(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }
}