using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Queries.Handler;

namespace Microwave.UnitTests
{
    [TestClass]
    public class LocalEventFeedTests
    {
        [TestMethod]

        public async Task EventFeedsParsesEventCorrectly()
        {
            var eventRepositoryInMemory = new EventRepositoryInMemory();
            var domainEvents = new List<IDomainEvent> { new UnitTestsPublished.EventPublishedAndSubscribed("123") };
            (await eventRepositoryInMemory.AppendAsync(domainEvents, 0)).Check();
            var localEventFeed = new LocalEventFeed<AsyncEventHandler<UnitTestsSubscribed.EventPublishedAndSubscribed>>(eventRepositoryInMemory);
            var eventsAsync = (await localEventFeed.GetEventsAsync()).ToList();

            Assert.AreEqual(
                typeof(UnitTestsSubscribed.EventPublishedAndSubscribed),
                eventsAsync.Single().DomainEvent.GetType());
        }
    }
}