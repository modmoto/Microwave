
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.DependencyInjectionExtensions;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        EventLocationConfig config = new EventLocationConfig(new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build());

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
            Assert.AreEqual("http://localhost:5000/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForReadModels()
        {
            var domainEventClient = new DomainEventClient<ReadModelHandler<IdQuery>>(config);
            Assert.AreEqual("http://lulsreadmodel.de/Api/DomainEvents", domainEventClient.BaseAddress.ToString());
        }
    }

    public class Ev2 : IDomainEvent
    {
        public Guid EntityId { get; }
    }

    public class IdQuery : ReadModel
    {
    }

    public class Ev1 : IDomainEvent
    {
        public Guid EntityId { get; }
    }

    public class Q1 : Query
    {
    }
}