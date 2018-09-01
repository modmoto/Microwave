using System;
using Adapters.Framework.EventStores;
using Domain.Framework;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventSourcingApplyStrategyTests
    {
        [Fact]
        public void ApplyGenericMethod()
        {
            var testUser = new TestUser();
            var newGuid = Guid.NewGuid();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(newGuid));
            Assert.Equal(newGuid, testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_NoApplyMethodFound()
        {
            var testUser = new TestUser();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserNeverDidThatEvent(Guid.NewGuid()));
            Assert.Equal(new Guid(), testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_ApplyMethodWithNotParameter()
        {
            var testUser = new TestUserWithNoApplyMethod();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(Guid.NewGuid()));
            Assert.Equal(new Guid(), testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_ApplyMethodWithMultipleApplyParameters()
        {
            var testUser = new TestUserMultipleNoApplyMethod();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(Guid.NewGuid()));
            Assert.Equal(new Guid(), testUser.Id);
        }
    }

    internal class TestUserMultipleNoApplyMethod : Entity
    {
        public void Apply(TestUserCreatedEvent domainEvent, TestUserNeverDidThatEvent didThatEvent)
        {
        }
    }

    internal class TestUserWithNoApplyMethod : Entity
    {
        public void Apply()
        {
        }
    }

    internal class TestUserNeverDidThatEvent : DomainEvent
    {
        public TestUserNeverDidThatEvent(Guid newGuid) : base(newGuid)
        {
        }
    }

    internal class TestUserCreatedEvent : DomainEvent
    {
        public TestUserCreatedEvent(Guid newGuid) : base(newGuid)
        {
        }
    }

    internal class TestUser : Entity
    {
        public TestUser()
        {
        }

        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }
}