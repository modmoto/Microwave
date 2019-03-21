using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Discovery;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        [TestMethod]
        public void ClientForQueries()
        {
            var eventLocation = new EventLocation();
            eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                new Uri("http://luls.de/"),
                new []{ nameof(Ev1) },
                new List<ReadModelSubscription>()));

            var domainEventClient = new DomainEventClient<QueryEventHandler<Q1, Ev1>>(eventLocation);
            Assert.AreEqual("http://luls.de/Api/DomainEventTypeStreams/Ev1", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForAsyncHandles()
        {
            var eventLocation = new EventLocation();
            eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                new Uri("http://troll.de/"),
                new []{ nameof(Ev2)},
                new List<ReadModelSubscription>()));
            var domainEventClient = new DomainEventClient<AsyncEventHandler<Ev2>>(eventLocation);
            Assert.AreEqual("http://troll.de/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForReadModels()
        {
            var eventLocation = new EventLocation();
            eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                new Uri("http://troll.de"),
                new List<string>(),
                new List<ReadModelSubscription> { new ReadModelSubscription(nameof(IdQuery), "wutEvent")}));
            var domainEventClient = new DomainEventClient<ReadModelHandler<IdQuery>>(eventLocation);
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
    }

    public class Ev1 : IDomainEvent
    {
        public Identity EntityId { get; }
    }

    public class Q1 : Query
    {
    }
}