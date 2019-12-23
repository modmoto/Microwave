using Microwave.Domain.EventSourcing;

namespace ModolithService
{
    public class EntityTest : Entity, IApply<Event2>
    {
        public void Apply(Event2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; private set; }
    }
}