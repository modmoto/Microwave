using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Validation;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class DomainResultTests
    {
        [TestMethod]
        public void CreatOkResult()
        {
            var domainResult = TestEntity.Create(GuidIdentity.Create());

            Assert.IsTrue(domainResult.IsOk);
            Assert.AreEqual(1, domainResult.DomainEvents.Count());
        }

        [TestMethod]
        public void CreatErrorResult()
        {
            var testEntity = new TestEntity(GuidIdentity.Create());
            var changeId = testEntity.ChangeId(StringIdentity.Create("NotWorking"));

            Assert.IsFalse(changeId.IsOk);
            Assert.AreEqual(1, changeId.DomainErrors.Count());

            Assert.ThrowsException<DomainValidationException>(() => changeId.DomainEvents.Count());
        }
    }

    public class TestEntity : Entity
    {
        public GuidIdentity Identity { get; }

        public TestEntity(GuidIdentity identity)
        {
            Identity = identity;
        }

        public static DomainResult Create(GuidIdentity identity)
        {
            return DomainResult.Ok(new EntityCreated(identity));
        }

        public DomainResult ChangeId(Identity identity)
        {
            if (identity is StringIdentity) return DomainResult.Error(new CanNotChangeToStringId());
            return DomainResult.Ok(new IdChanged(identity));
        }

        public Identity Id { get; set; }
    }

    public class IdChanged : IDomainEvent
    {
        public Identity Identity { get; }

        public IdChanged(Identity identity)
        {
            Identity = identity;
        }

        public Identity EntityId => Identity;
    }

    public class CanNotChangeToStringId : DomainError
    {
        public CanNotChangeToStringId() : base("You can not change to a string ID")
        {
        }
    }

    public class EntityCreated : IDomainEvent
    {
        public GuidIdentity Identity { get; }

        public EntityCreated(GuidIdentity identity)
        {
            Identity = identity;
        }

        public Identity EntityId => Identity;
    }
}