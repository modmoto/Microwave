using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Eventstores.UnitTests;

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

            Assert.AreEqual(guidIdentity, readModelTestForQuery.Id);
        }

        [TestMethod]
        public void QueryTest_Versioned_HappyPath()
        {
            var readModelTestForQuery = new ReadModelTestForQuery();

            var guidIdentity = GuidIdentity.Create();
            readModelTestForQuery.Handle(new TestEv2(guidIdentity), 14);

            Assert.AreEqual(guidIdentity, readModelTestForQuery.Id);
            Assert.AreEqual(14, readModelTestForQuery.Version);
        }
    }

    internal class ReadModelTestForQuery : ReadModel, IHandle<TestEv>, IHandleVersioned<TestEv2>
    {
        public override Type GetsCreatedOn { get; }
        public void Handle(TestEv domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public void Handle(TestEv2 domainEvent, long version)
        {
            Id = domainEvent.EntityId;
            Version = version;
        }

        public long Version { get; set; }

        public Identity Id { get; set; }
    }
}