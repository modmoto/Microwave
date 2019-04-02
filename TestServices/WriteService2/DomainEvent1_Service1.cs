using Microwave.Domain;

namespace WriteService2
{
    public class Entity2 : Entity, IApply<DomainEvent1_Service2>, IApply<DomainEvent2_Service2>
    {
        public void Apply(DomainEvent1_Service2 domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(DomainEvent2_Service2 domainEvent)
        {
            throw new System.NotImplementedException();
        }
    }
    public class DomainEvent1_Service2 : IDomainEvent
    {
        public DomainEvent1_Service2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class DomainEvent2_Service2 : IDomainEvent
    {
        public DomainEvent2_Service2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}