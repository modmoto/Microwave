using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Discovery.Domain;
using Microwave.Discovery.Domain.Services;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.WebApi.Querries;
using Moq;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DomainEventClientTest
    {
        [TestMethod]
        public void ClientForQueries()
        {
            var mock = new Mock<IEventLocation>();
            mock.Setup(m => m.GetServiceForEvent(typeof(Ev1))).Returns(new ServiceNode(new ServiceEndPoint(new Uri
            ("http://luls.de/")), null, null));
            var domainEventClient = new DomainEventClient<QueryEventHandler<Q1, Ev1>>(mock.Object);
            Assert.AreEqual("http://luls.de/Api/DomainEventTypeStreams/Ev1", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForAsyncHandles()
        {
            var mock = new Mock<IEventLocation>();
            mock.Setup(m => m.GetServiceForEvent(typeof(Ev2))).Returns(new ServiceNode(new ServiceEndPoint(new Uri
                ("http://troll.de/")), null, null));

            var domainEventClient = new DomainEventClient<AsyncEventHandler<Ev2>>(mock.Object);
            Assert.AreEqual("http://troll.de/Api/DomainEventTypeStreams/Ev2", domainEventClient.BaseAddress.ToString());
        }

        [TestMethod]
        public void ClientForReadModels()
        {
            var mock = new Mock<IEventLocation>();
            mock.Setup(m => m.GetServiceForReadModel(typeof(IdQuery))).Returns(new ServiceNode(new ServiceEndPoint(new Uri
                ("http://troll2.de/")), null, null));

            var domainEventClient = new DomainEventClient<ReadModelHandler<IdQuery>>(mock.Object);
            Assert.AreEqual("http://troll2.de/Api/DomainEvents", domainEventClient.BaseAddress.ToString());
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