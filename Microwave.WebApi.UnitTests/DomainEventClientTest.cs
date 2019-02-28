using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        ReadModelConfiguration config = new ReadModelConfiguration(new Uri("http://troll.de"))
        {
            DomainEventConfig = new DomainEventConfig()
            {
                { typeof(Ev1), new Uri("http://luls.de")}
            },
            ReadModelConfig = new ReadModelConfig()
            {
                { typeof(Ev1), new Uri("http://lulsReadModel.de")}

            }
        };

        [TestMethod]
        public void ClientForQueries()
        {
            var domainEventClient = new DomainEventClient<QueryEventHandler<Q1, Ev1>>(config);
            Assert.AreEqual("http://luls.de/Api/DomainEventTypeStreams/Ev1", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForAsyncHandles()
        {
            var domainEventClient = new DomainEventClient<AsyncEventHandler<Ev2>>(config);
            Assert.AreEqual("http://troll.de/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForReadModels()
        {
            var domainEventClient = new DomainEventClient<ReadModelHandler<IdQuery>>(config);
            Assert.AreEqual("http://troll.de/Api/DomainEvents", domainEventClient.BaseAddress.ToString());
        }
    }

    public class Ev2 : IDomainEvent
    {
        public Identity EntityId { get; }
    }

    public class IdQuery : ReadModel
    {
        public override Type GetsCreatedOn { get; }
        public override Identity EntityId { get; }
    }

    public class Ev1 : IDomainEvent
    {
        public Identity EntityId { get; }
    }

    public class Q1 : Query
    {
    }
}