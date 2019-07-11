using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Queries;
using Microwave.WebApi.Querries;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        [TestMethod]
        public void ClientForQueries()
        {
            var domainEventClient = new DomainEventClient<QueryEventHandler<Q1, Ev1>>(new EventLocationFake());
            Assert.AreEqual("http://luls.de/Api/DomainEventTypeStreams/Ev1", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForAsyncHandles()
        {
            var domainEventClient = new DomainEventClient<AsyncEventHandler<Ev2>>(new EventLocationFake());
            Assert.AreEqual("http://troll.de/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForReadModels()
        {
            var domainEventClient = new DomainEventClient<ReadModelHandler<IdQuery>>(new EventLocationFake());
            Assert.AreEqual("http://troll2.de/Api/DomainEvents", domainEventClient.BaseAddress.ToString());
        }
    }

    internal class EventLocationFake : EventLocation
    {
        public EventLocationFake() : base(new List<MicrowaveServiceNode>
        {
            MicrowaveServiceNode.Create(new ServiceEndPoint(new Uri("http://luls.de/")),
                new List<EventSchema>
            {
                new EventSchema(nameof(Ev1))
            }, new List<ReadModelSubscription>()),
            MicrowaveServiceNode.Create(new ServiceEndPoint(new Uri("http://troll.de/")),
                new List<EventSchema>
                {
                    new EventSchema(nameof(Ev2))
                }, new List<ReadModelSubscription>()),
            MicrowaveServiceNode.Create(new ServiceEndPoint(new Uri("http://troll2.de/")),
                new List<EventSchema>(),
                new List<ReadModelSubscription>
            {
                new ReadModelSubscription(nameof(IdQuery), new EventSchema(nameof(Ev2)))
            })
        }, new List<EventSchema>(), new List<ReadModelSubscription>())
        {
        }
    }

    public class Ev2 : IDomainEvent, ISubscribedDomainEvent
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