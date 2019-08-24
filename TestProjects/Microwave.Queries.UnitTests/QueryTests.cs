using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Identities;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void QueryTest_HappyPath()
        {
            var readModelTestForQuery = new ReadModelTestForQuery();

            var guidIdentity = GuidIdentity.Create();
            readModelTestForQuery.Handle(new TestEv(guidIdentity), 12);

            Assert.AreEqual(guidIdentity, readModelTestForQuery.EntityId);
        }

        [TestMethod]
        public void QueryTest_Versioned_HappyPath()
        {
            var readModelTestForQuery = new ReadModelTestForQuery();

            var guidIdentity = GuidIdentity.Create();
            readModelTestForQuery.Handle(new TestEv2(guidIdentity), 14);

            Assert.AreEqual(guidIdentity, readModelTestForQuery.EntityId);
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

        public Identity EntityId { get; set; }
    }
}