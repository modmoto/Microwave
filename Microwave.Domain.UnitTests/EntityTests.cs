using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class EntityTests
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
        public void ApplyGenericMethod_PrivateApply()
        {
            var testUser = (Entity) new TestUser_ProtectedApply();
            var newGuid = Guid.NewGuid();
            testUser.Apply(new [] { new TestUserCreatedEvent(newGuid)});
            Assert.AreEqual(newGuid, ((TestUser_ProtectedApply) testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_MultipleApply()
        {
            var testUser = (Entity) new TestUser_MultipleApply();
            var newGuid = Guid.NewGuid();
            var secondGuid = Guid.NewGuid();
            testUser.Apply(new List<IDomainEvent> { new TestUserCreatedEvent(newGuid), new TestUserNeverDidThatEvent(secondGuid) });
            Assert.AreEqual(secondGuid, ((TestUser_MultipleApply) testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ProtectedApply()
        {
            var testUser = (Entity) new TestUser_ProtectedApply();
            var newGuid = Guid.NewGuid();
            testUser.Apply(new [] { new TestUserCreatedEvent(newGuid)});
            Assert.AreEqual(newGuid, ((TestUser_ProtectedApply) testUser).Id);
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

    public class TestUser_PrivateApply : Entity
    {
        private void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
    }

    public class TestUser_MultipleApply : Entity
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public void Apply(TestUserNeverDidThatEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
    }

    public class TestUser_ProtectedApply : Entity
    {
        protected void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
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

    public class TestUserNeverDidThatEvent : IDomainEvent
    {
        public TestUserNeverDidThatEvent(Guid newGuid)
        {
            EntityId = newGuid;
        }

        public Guid EntityId { get; }
    }

    public class TestUserCreatedEvent : IDomainEvent
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