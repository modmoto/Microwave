using System;
using Xunit;

namespace Domain.Framework.Tests
{
    public class EntityTests
    {
        [Fact]
        public void ApplyGenericMethod()
        {
            var testUser = (Entity) new TestUser();
            var newGuid = Guid.NewGuid();
            testUser.Apply(new TestUserCreatedEvent(newGuid));
            Assert.Equal(newGuid, testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_NoApplyMethodFound()
        {
            var testUser = (Entity) new TestUser();
            testUser.Apply(new TestUserNeverDidThatEvent(Guid.NewGuid()));
            Assert.Equal(new Guid(), testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_ApplyMethodWithNotParameter()
        {
            var testUser = (Entity) new TestUserWithNoApplyMethod();
            testUser.Apply(new TestUserCreatedEvent(Guid.NewGuid()));
            Assert.Equal(new Guid(), testUser.Id);
        }

        [Fact]
        public void ApplyGenericMethod_ApplyMethodWithMultipleApplyParameters()
        {
            var testUser = (Entity) new TestUserMultipleNoApplyMethod();
            testUser.Apply(new TestUserCreatedEvent(Guid.NewGuid()));
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
        public void Apply(TestUserCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }
}