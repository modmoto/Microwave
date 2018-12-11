using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class DomainInterfaceTests
    {
        [TestMethod]
        public void EqualsEvent()
        {
            var testEvent = new TestEvent(new Identifier<string>("123"));
            var testEvent2 = new TestEvent(new Identifier<string>("123"));

            Assert.IsTrue(testEvent.EntityId.IsSameId(testEvent2.EntityId));
        }

        [TestMethod]
        public void NotEqualsEvent()
        {
            var testEvent = new TestEvent(new Identifier<string>("123"));
            var testEvent2 = new TestEvent(new Identifier<string>("13"));

            Assert.IsFalse(testEvent.EntityId.IsSameId(testEvent2.EntityId));
        }

        [TestMethod]
        public void NotEqualsEventGuid()
        {
            var testEvent = new TestEvent(new Identifier<Guid>(Guid.NewGuid()));
            var testEvent2 = new TestEvent(new Identifier<Guid>(Guid.NewGuid()));

            Assert.IsFalse(testEvent.EntityId.IsSameId(testEvent2.EntityId));
        }


        [TestMethod]
        public void EqualsEventGuid()
        {
            var newGuid = Guid.NewGuid();
            var testEvent = new TestEvent(new Identifier<Guid>(newGuid));
            var testEvent2 = new TestEvent(new Identifier<Guid>(newGuid));

            Assert.IsTrue(testEvent.EntityId.IsSameId(testEvent2.EntityId));
        }
    }

    public class TestEvent : IIdentifiableDomainEvent
    {
        public TestEvent(IIdentifiable entityId)
        {
            EntityId = entityId;
        }

        public IIdentifiable EntityId { get; }
    }
}