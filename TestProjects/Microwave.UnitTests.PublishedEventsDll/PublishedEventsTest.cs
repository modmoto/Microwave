using System.Collections.Generic;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Queries;

namespace Microwave.UnitTests.PublishedEventsDll
{
    public class TestEntity1 : IApply, IApply<TestDomainEvent_PublishedEvent1>
    {
        public void Apply(IEnumerable<IDomainEvent> domainEvents)
        {
        }

        public void Apply(TestDomainEvent_PublishedEvent1 domainEvent)
        {
        }
    }

    public class TestEntityThatShouldNotGoIntoReadModelRegistration : IApply,
    IApply<TestEntityThatShouldNotGoIntoReadModelRegistrationEvent>
        {
            public void Apply(IEnumerable<IDomainEvent> domainEvents)
            {
            }

            public void Apply(TestEntityThatShouldNotGoIntoReadModelRegistrationEvent domainEvent)
            {
            }
        }

    public class TestEntity1_AddedTwice : IApply, IApply<TestDomainEvent_PublishedEvent1>
    {
        public void Apply(IEnumerable<IDomainEvent> domainEvents)
        {
        }

        public void Apply(TestDomainEvent_PublishedEvent1 domainEvent)
        {
        }
    }

    public class TestEntity_NotImplementingIApply : IApply<TestDomainEvent_PublishedEvent2>
    {
        public void Apply(TestDomainEvent_PublishedEvent2 domainEvent)
        {
        }
    }

    public class TestReadModel_NotImplementingIApply : ReadModel<TestDomainEvent_OnlySubscribedEvent>, IHandle<TestDomainEvent_OnlySubscribedEvent>
    {
        public void Handle(TestDomainEvent_OnlySubscribedEvent domainEvent)
        {
        }
    }

    public class TestQuerry_NotImplementingIApply : Query, IHandle<TestDomainEvent_OnlySubscribedEvent>
    {
        public void Handle(TestDomainEvent_OnlySubscribedEvent domainEvent)
        {
        }
    }

    public class TestEntityThatShouldNotGoIntoReadModelRegistrationEvent : IDomainEvent
    {
        public Identity EntityId { get; }
    }
}