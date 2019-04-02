using Microwave.Domain;

namespace WriteService1
{
    public class Entity1 : Entity, IApply<DomainEvent1_Service1>, IApply<DomainEvent2_Service1>
    {
        public void Apply(DomainEvent1_Service1 domainEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(DomainEvent2_Service1 domainEvent)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DomainEvent1_Service1 : IDomainEvent
    {
        public DomainEvent1_Service1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class DomainEvent2_Service1 : IDomainEvent
    {
        public DomainEvent2_Service1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}