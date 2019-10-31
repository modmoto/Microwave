using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class EntityTests
    {
        [TestMethod]
        public void ApplyGenericMethod()
        {
            var testUser = (Entity) new TestUserNormal();
            var newGuid = Guid.NewGuid();
            testUser.Apply(new [] { new TestUserCreatedEvent(newGuid)});
            Assert.AreEqual(newGuid.ToString(), ((TestUserNormal)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_MultipleApply()
        {
            var testUser = (Entity) new TestUser_MultipleApply();
            var newGuid = Guid.NewGuid();
            var secondGuid = Guid.NewGuid();
            testUser.Apply(new List<IDomainEvent> { new TestUserCreatedEvent(newGuid), new TestUserNeverDidThatEvent(secondGuid) });
            Assert.AreEqual(secondGuid.ToString(), ((TestUser_MultipleApply) testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_NoApplyMethodFound()
        {
            var testUser = (Entity) new TestUser();
            testUser.Apply(new [] { new TestUserNeverDidThatEvent(Guid.NewGuid()) });
            Assert.AreEqual(null, ((TestUser)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_ApplyMethodWithNotParameter()
        {
            var testUser = (Entity) new TestUserWithNoApplyMethod();
            testUser.Apply(new [] { new TestUserCreatedEvent(Guid.NewGuid()) } );
            Assert.AreEqual(Guid.Empty, ((TestUserWithNoApplyMethod)testUser).Id);
        }

        [TestMethod]
        public void ApplyGenericMethod_PrivateApplyWithoutInterface()
        {
            var testUser = (Entity) new TestUser_PrivateApply();
            testUser.Apply(new [] { new TestUserCreatedEvent(Guid.NewGuid()) } );
            Assert.AreEqual(null, ((TestUser_PrivateApply)testUser).Id);
        }
    }

    public class TestUser_PrivateApply : Entity
    {
        private void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
    }

    public class TestUser_MultipleApply : Entity, IApply<TestUserCreatedEvent>, IApply<TestUserNeverDidThatEvent>
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public void Apply(TestUserNeverDidThatEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
    }

    public class TestUserWithNoApplyMethod : Entity
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
            EntityId = newGuid.ToString();
        }

        public string EntityId { get; }
    }

    public class TestUserCreatedEvent : IDomainEvent
    {
        public TestUserCreatedEvent(Guid newGuid)
        {
            EntityId = newGuid.ToString();
        }

        public string EntityId { get; }
    }

    public class TestUser : Entity
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
    }

    public class TestUserNormal : Entity, IApply<TestUserCreatedEvent>
    {
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
    }
}