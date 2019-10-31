using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Validation;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class DomainResultTests
    {
        [TestMethod]
        public void CreatOkResult()
        {
            var domainResult = TestEntity.Create(Guid.NewGuid());

            Assert.IsTrue(domainResult.IsOk);
            Assert.AreEqual(1, domainResult.DomainEvents.Count());
        }

        [TestMethod]
        public void CreatErrorResult()
        {
            var testEntity = new TestEntity(Guid.NewGuid());
            var changeId = testEntity.ChangeId(null);

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
        public string Identity { get; }

        public TestEntity(Guid identity)
        {
            Identity = identity.ToString();
        }

        public static DomainResult Create(Guid identity)
        {
            return DomainResult.Ok(new EntityCreated(identity));
        }

        public DomainResult ChangeId(string identity)
        {
            if (identity == null) return DomainResult.Error(new CanNotUSeNullId());
            return DomainResult.Ok(new IdChanged(identity));
        }

        public string Id { get; set; }
    }

    public class IdChanged : IDomainEvent
    {
        public string Identity { get; }

        public IdChanged(string identity)
        {
            Identity = identity;
        }

        public string EntityId => Identity;
    }

    public class CanNotUSeNullId : DomainError
    {
        public CanNotUSeNullId() : base("You can not change to a string ID")
        {
        }
    }

    public class EntityCreated : IDomainEvent
    {
        public Guid Identity { get; }

        public EntityCreated(Guid identity)
        {
            Identity = identity;
        }

        public string EntityId => Identity.ToString();
    }
}