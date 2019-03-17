using Microwave.Domain;

namespace Microwave.UnitTests.PublishedEventsDll
{
    public class TestDomainEvent_PublishedEvent1 : IDomainEvent
    {
        public TestDomainEvent_PublishedEvent1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestDomainEvent_PublishedEvent3 : IDomainEvent
    {
        public TestDomainEvent_PublishedEvent3(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
    
    public class TestDomainEvent_PublishedEvent2 : IDomainEvent
    {
        public TestDomainEvent_PublishedEvent2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}