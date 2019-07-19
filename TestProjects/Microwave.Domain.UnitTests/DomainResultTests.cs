using System;
using System.Collections.Generic;
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

        [TestMethod]
        public void CreatTypelessErrorResult()
        {
            var domainDomainErrorKey = "errorKey";
            var domainResult = DomainResult.Error(domainDomainErrorKey);

            var domainError = domainResult.DomainErrors.Single();

            Assert.AreEqual(domainError.ErrorType, domainDomainErrorKey);
        }

        [TestMethod]
        public void CreatTypelessErrorResultList()
        {
            var domainDomainErrorKey = "errorKey";
            var domainResult = DomainResult.Error(new List<string> {domainDomainErrorKey, domainDomainErrorKey});

            var domainError = domainResult.DomainErrors.First();

            Assert.AreEqual(domainError.ErrorType, domainDomainErrorKey);
        }

        [TestMethod]
        public void CreatEnumErrorResultList()
        {
            var domainResult = DomainResult.Error(new List<Enum> {MyTestEnum.WhateverError, MyTestEnum.WhateverError});

            var domainError = domainResult.DomainErrors.First();

            Assert.AreEqual(domainError.ErrorType, MyTestEnum.WhateverError.ToString());
        }

        [TestMethod]
        public void CreatEnumErrorResult()
        {
            var domainResult = DomainResult.Error(MyTestEnum.WhateverError);

            var domainError = domainResult.DomainErrors.Single();

            Assert.AreEqual(domainError.ErrorType, MyTestEnum.WhateverError.ToString());
        }
    }

    public enum MyTestEnum
    {
        WhateverError
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

    public class CanNotChangeToStringId : DomainErrorRenamed
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