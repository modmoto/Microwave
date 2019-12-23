using Microwave.Domain.EventSourcing;

namespace ModolithService
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