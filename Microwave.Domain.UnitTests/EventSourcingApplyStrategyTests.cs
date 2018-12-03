using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class EventSourcingApplyStrategyTests
    {
        [TestMethod]
        public void ApplyGenericMethod()
        {
            var testUser = (Entity) new TestUser();
            var newGuid = Guid.NewGuid();
            testUser.Apply(new [] { new TestUserCreatedEvent(newGuid)});
            Assert.AreEqual(newGuid, ((TestUser)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_NoApplyMethodFound()
        {
            var testUser = (Entity) new TestUser();
            testUser.Apply(new [] { new TestUserNeverDidThatEvent(Guid.NewGuid()) });
            Assert.AreEqual(Guid.Empty, ((TestUser)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ApplyMethodWithNotParameter()
        {
            var testUser = (Entity) new TestUserWithNoApplyMethod();
            testUser.Apply(new [] { new TestUserCreatedEvent(Guid.NewGuid()) } );
            Assert.AreEqual(Guid.Empty, ((TestUserWithNoApplyMethod)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ApplyMethodWithMultipleApplyParameters()
        {
            var testUser = (Entity) new TestUserMultipleNoApplyMethod();
            testUser.Apply(new [] { new TestUserCreatedEvent(Guid.NewGuid()) } );
            Assert.AreEqual(Guid.Empty, ((TestUserMultipleNoApplyMethod)testUser).Id);
        }
    }

    internal class TestUserMultipleNoApplyMethod : Entity
    {
        public void Apply(TestUserCreatedEvent domainEvent, TestUserNeverDidThatEvent didThatEvent)
        {
        }

        public Guid Id { get; set; }
    }

    internal class TestUserWithNoApplyMethod : Entity
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

    internal class TestUser : Entity
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
    }
}