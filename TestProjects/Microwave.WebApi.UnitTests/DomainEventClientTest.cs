using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.WebApi.Queries;
using Moq;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        [TestMethod]
        public async Task ClientForQueries()
        {
            var mock = new Mock<IStatusRepository>();
            mock.Setup(s => s.GetEventLocation()).ReturnsAsync(new EventLocationFake());
            var domainEventFactory = new DomainEventClientFactory(mock.Object, new DefaultMicrowaveHttpClientFactory());
            var domainEventClient = await domainEventFactory.GetClient<QueryEventHandler<Q1, Ev1>>();
            Assert.AreEqual("http://luls.de/Api/DomainEventTypeStreams/Ev1", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public async Task ClientForAsyncHandles()
        {
            var mock = new Mock<IStatusRepository>();
            mock.Setup(s => s.GetEventLocation()).ReturnsAsync(new EventLocationFake());
            var domainEventFactory = new DomainEventClientFactory(mock.Object, new DefaultMicrowaveHttpClientFactory());
            var domainEventClient = await domainEventFactory.GetClient<AsyncEventHandler<Ev2>>();
            Assert.AreEqual("http://troll.de/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public async Task ClientForReadModels()
        {
            var mock = new Mock<IStatusRepository>();
            mock.Setup(s => s.GetEventLocation()).ReturnsAsync(new EventLocationFake());
            var domainEventFactory = new DomainEventClientFactory(mock.Object, new DefaultMicrowaveHttpClientFactory());
            var domainEventClient = await domainEventFactory.GetClient<ReadModelEventHandler<IdQuery>>();
            Assert.AreEqual("http://troll2.de/Api/DomainEvents", domainEventClient.BaseAddress.ToString());
        }
    }

    public class EventLocationFake : EventLocation
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