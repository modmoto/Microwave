using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework.Exceptions;
using Application.Framework.Results;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Adapters.Framework.Eventstores.UnitTests
{
    public class EventRepositoryTests
    {
        [Test]
        public async Task AddAndLoadEvents()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EventRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var res = await eventRepository.AppendAsync(events, -1);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreEqual(0, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].EntityId);
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[1].Version);
        }

        [Test]
        public async Task AddAndLoadEventsConcurrent()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddAndLoadEventsConcurrent")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EventRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, -1);
            var t2 = eventRepository.AppendAsync(events2, -1);

            var allResults = await Task.WhenAll(t1, t2);
            Assert.Throws<ConcurrencyException>(() => CheckAllResults(allResults));
        }

        private static void CheckAllResults(Result[] whenAll)
        {
            foreach (var result in whenAll)
            {
                result.Check();
            }
        }
    }

    public class TestEvent1 : DomainEvent
    {
        public TestEvent1(Guid entityId) : base(entityId)
        {
        }
    }

    public class TestEvent2 : DomainEvent
    {
        public TestEvent2(Guid entityId) : base(entityId)
        {
        }
    }
}