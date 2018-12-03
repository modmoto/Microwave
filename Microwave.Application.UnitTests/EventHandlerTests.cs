using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Ports;
using Microwave.Domain;
using Microwave.EventStores;
using Moq;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class EventHandlerTests
    {
        [TestMethod]
        public async Task HandleIsOnlyCalledOnce()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("HandleIsOnlyCalledOnce")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventFeedMock = new Mock<IEventFeed<TestEv2>>();
            var domainEventWrapper = new DomainEventWrapper<TestEv2>
            {
                Created = 12,
                DomainEvent = new TestEv2()
            };

            eventFeedMock.Setup(feed => feed.GetEventsByTypeAsync(0)).ReturnsAsync(new[] { domainEventWrapper });

            var handleAsync = new Handler1();
            var handleAsync2 = new Handler2();
            var eventDelegateHandler = new EventDelegateHandler<TestEv2>(
                new VersionRepository(eventStoreContext),
                eventFeedMock.Object,
                new List<IHandleAsync<TestEv2>> {handleAsync, handleAsync2});

            await eventDelegateHandler.Update();
            await eventDelegateHandler.Update();

            Assert.AreEqual(1, handleAsync.TimesCalled);
            Assert.AreEqual(1, handleAsync.TimesCalled);
        }
    }

    public class Handler1 : IHandleAsync<TestEv2>
    {
        public int TimesCalled { get; set; }

        public Task HandleAsync(TestEv2 domainEvent)
        {
            TimesCalled = TimesCalled + 1;
            return Task.CompletedTask;
        }
    }

    public class Handler2 : IHandleAsync<TestEv2>
    {
        public int TimesCalled { get; set; }

        public Task HandleAsync(TestEv2 domainEvent)
        {
            TimesCalled = TimesCalled + 1;
            return Task.CompletedTask;
        }
    }

    public class TestEv1 : IDomainEvent
    {
        public Guid EntityId { get; }
    }

    public class TestEv2 : IDomainEvent
    {
        public Guid EntityId { get; }
    }
}