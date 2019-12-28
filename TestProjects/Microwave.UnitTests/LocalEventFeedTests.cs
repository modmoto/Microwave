using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Queries;
using Microwave.Queries.Handler;

namespace Microwave.UnitTests
{
    [TestClass]
    public class LocalEventFeedTests
    {
        [TestMethod]
        public async Task EventFeedsParsesEventCorrectly_AsyncEventHandler()
        {
            var eventRepositoryInMemory = new EventRepositoryInMemory();
            var domainEvents = new List<IDomainEvent> { new UnitTestsPublished.EventPublishedAndSubscribed("123",
            "add") };
            (await eventRepositoryInMemory.AppendAsync(domainEvents, 0)).Check();
            var localEventFeed = new LocalEventFeed<AsyncEventHandler<UnitTestsSubscribed.EventPublishedAndSubscribed>>(eventRepositoryInMemory);
            var eventsAsync = (await localEventFeed.GetEventsAsync()).ToList();

            var subscribedDomainEvent = eventsAsync.Single().DomainEvent as UnitTestsSubscribed.EventPublishedAndSubscribed;
            Assert.AreEqual("add", subscribedDomainEvent.Addition);
        }

        [TestMethod]
        public async Task EventFeedsParsesEventCorrectly_QuerryHandler()
        {
            var eventRepositoryInMemory = new EventRepositoryInMemory();
            var domainEvents = new List<IDomainEvent> { new UnitTestsPublished.EventPublishedAndSubscribed("123",
                "add") };
            (await eventRepositoryInMemory.AppendAsync(domainEvents, 0)).Check();
            var localEventFeed = new LocalEventFeed<QueryEventHandler<QeryForModolith, UnitTestsSubscribed
            .EventPublishedAndSubscribed>>(eventRepositoryInMemory);
            var eventsAsync = (await localEventFeed.GetEventsAsync()).ToList();

            var subscribedDomainEvent = eventsAsync.Single().DomainEvent as UnitTestsSubscribed.EventPublishedAndSubscribed;
            Assert.AreEqual("add", subscribedDomainEvent.Addition);
        }

        [TestMethod]
        public async Task EventFeedsParsesEventCorrectly_ReadModel()
        {
            var eventRepositoryInMemory = new EventRepositoryInMemory();
            var domainEvents = new List<IDomainEvent> { new UnitTestsPublished.EventPublishedAndSubscribed("123",
                "add") };
            (await eventRepositoryInMemory.AppendAsync(domainEvents, 0)).Check();
            var localEventFeed = new LocalEventFeed<ReadModelEventHandler<ReadModelForModolith>>(eventRepositoryInMemory);
            var eventsAsync = (await localEventFeed.GetEventsAsync()).ToList();

            var subscribedDomainEvent = eventsAsync.Single().DomainEvent as UnitTestsSubscribed.EventPublishedAndSubscribed;
            Assert.AreEqual("add", subscribedDomainEvent.Addition);
        }
    }

    public class ReadModelForModolith :
        ReadModel<UnitTestsSubscribed.EventPublishedAndSubscribed>,
        IHandle<UnitTestsSubscribed.EventPublishedAndSubscribed>
    {
        public void Handle(UnitTestsSubscribed.EventPublishedAndSubscribed domainEvent)
        {
        }
    }

    public class QeryForModolith : Query
    {
    }
}