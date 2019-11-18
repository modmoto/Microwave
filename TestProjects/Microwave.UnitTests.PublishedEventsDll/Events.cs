using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Queries;

namespace Microwave.UnitTests.PublishedEventsDll
{
    public class TestDomainEvent_PublishedEvent1 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestDomainEvent_PublishedEvent1(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEvent : ISubscribedDomainEvent
    {
        public TestDomainEvent_OnlySubscribedEvent(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEvent_HandleAsync : ISubscribedDomainEvent
    {
        public TestDomainEvent_OnlySubscribedEvent_HandleAsync(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEvent_HandleAsync_Handler : IHandleAsync<TestDomainEvent_OnlySubscribedEvent_HandleAsync>
    {
        public Task HandleAsync(TestDomainEvent_OnlySubscribedEvent_HandleAsync domainEvent)
        {
            return null;
        }
    }
    
    public class TestDomainEvent_PublishedEvent2 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestDomainEvent_PublishedEvent2(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestEntityForSeed : Entity, IApply<TestDomainEvent_PublishedEvent2>
    {
        public string DomainEventEntityId { get; private set; }

        public void Apply(TestDomainEvent_PublishedEvent2 domainEvent)
        {
            DomainEventEntityId = domainEvent.EntityId;
        }
    }
}