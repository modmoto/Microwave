using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Queries;

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

    public class TestDomainEvent_OnlySubscribedEvent
    {
        public TestDomainEvent_OnlySubscribedEvent(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEvent_HandleAsync
    {
        public TestDomainEvent_OnlySubscribedEvent_HandleAsync(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEvent_HandleAsync_Handler : IHandleAsync<TestDomainEvent_OnlySubscribedEvent_HandleAsync>
    {
        public Task HandleAsync(TestDomainEvent_OnlySubscribedEvent_HandleAsync domainEvent)
        {
            return null;
        }
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