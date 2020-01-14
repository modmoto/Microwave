using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void QueryTest_HappyPath()
        {
            var readModelTestForQuery = new ReadModelTestForQuery();

            var guidstring = Guid.NewGuid();
            readModelTestForQuery.Handle(new TestEv(guidstring), 12);

            Assert.AreEqual(guidstring.ToString(), readModelTestForQuery.EntityId);
        }

        [TestMethod]
        public void QueryTest_Versioned_HappyPath()
        {
            var readModelTestForQuery = new ReadModelTestForQuery();

            var guidstring = Guid.NewGuid();
            readModelTestForQuery.Handle(new TestEv2(guidstring), 14);

            Assert.AreEqual(guidstring.ToString(), readModelTestForQuery.EntityId);
            Assert.AreEqual(14, readModelTestForQuery.InnerVersion);
        }
    }

    public class ReadModelTestForQuery : ReadModel<TestEv>, IHandle<TestEv>, IHandleVersioned<TestEv2>
    {
        public void Handle(TestEv domainEvent)
        {
            EntityId = domainEvent.EntityId;
        }

        public void Handle(TestEv2 domainEvent, long version)
        {
            EntityId = domainEvent.EntityId;
            InnerVersion = version;
        }

        public long InnerVersion { get; set; }

        public string EntityId { get; set; }
    }
}