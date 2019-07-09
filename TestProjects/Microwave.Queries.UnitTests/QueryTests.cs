using System;
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
            Assert.AreEqual(14, readModelTestForQuery.Version);
        }
    }

    internal class ReadModelTestForQuery : ReadModel, IHandle<TestEv>, IHandleVersioned<TestEv2>
    {
        public override Type GetsCreatedOn { get; }
        public void Handle(TestEv domainEvent)
        {
            EntityId = domainEvent.EntityId;
        }

        public void Handle(TestEv2 domainEvent, long version)
        {
            EntityId = domainEvent.EntityId;
            Version = version;
        }

        public long Version { get; set; }

        public Identity EntityId { get; set; }
    }
}