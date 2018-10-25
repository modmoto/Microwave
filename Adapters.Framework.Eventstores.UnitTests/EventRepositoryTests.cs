using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Adapters.Framework.Eventstores.UnitTests
{
    public class EventRepositoryTests
    {
        [Test]
        public async Task AddEvents()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EventRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var res = await eventRepository.AppendAsync(events, 0);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
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