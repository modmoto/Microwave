using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventSourcingApplyStrategyTests
    {
        [TestMethod]
        public void ApplyGenericMethod()
        {
            var testUser = new TestUser();
            var newGuid = Guid.NewGuid();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(newGuid));
            Assert.AreEqual(newGuid, testUser.Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_NoApplyMethodFound()
        {
            var testUser = new TestUser();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserNeverDidThatEvent(Guid.NewGuid()));
            Assert.AreEqual(new Guid(), testUser.Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ApplyMethodWithNotParameter()
        {
            var testUser = new TestUserWithNoApplyMethod();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(Guid.NewGuid()));
            Assert.AreEqual(new Guid(), testUser.Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ApplyMethodWithMultipleApplyParameters()
        {
            var testUser = new TestUserMultipleNoApplyMethod();
            var eventSourcingApplyStrategy = new EventSourcingApplyStrategy();
            eventSourcingApplyStrategy.Apply(testUser, new TestUserCreatedEvent(Guid.NewGuid()));
            Assert.AreEqual(new Guid(), testUser.Id);
        }
    }

    internal class TestUserMultipleNoApplyMethod
    {
        public void Apply(TestUserCreatedEvent domainEvent, TestUserNeverDidThatEvent didThatEvent)
        {
        }

        public Guid Id { get; set; }
    }

    internal class TestUserWithNoApplyMethod
    {
        public void Apply()
        {
        }

        public Guid Id { get; set; }
    }

    internal class TestUserNeverDidThatEvent : IDomainEvent
    {
        public TestUserNeverDidThatEvent(Guid newGuid)
        {
            EntityId = newGuid;
        }

        public Guid EntityId { get; }
    }

    internal class TestUserCreatedEvent : IDomainEvent
    {
        public TestUserCreatedEvent(Guid newGuid)
        {
            EntityId = newGuid;
        }

        public Guid EntityId { get; }
    }

    internal class TestUser : IApply<TestUserCreatedEvent>
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
    }
}