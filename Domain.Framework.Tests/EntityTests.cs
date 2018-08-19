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