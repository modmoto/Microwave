using System;
using System.Collections.Generic;
using Microwave.Domain;
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

    public class TestEntity3 : Entity, IApply<TestDomainEvent_PublishedEvent3>
    {
        public void Apply(TestDomainEvent_PublishedEvent3 domainEvent)
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

    public class TestReadModel_NotImplementingIApply : ReadModel, IApply<TestDomainEvent_PublishedEvent3>
    {
        public void Apply(TestDomainEvent_PublishedEvent3 domainEvent)
        {
        }

        public override Type GetsCreatedOn { get; }
    }

    public class TestQuerry_NotImplementingIApply : Query, IApply<TestDomainEvent_PublishedEvent3>
    {
        public void Apply(TestDomainEvent_PublishedEvent3 domainEvent)
        {
        }
    }
}