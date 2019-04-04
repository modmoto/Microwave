using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Eventstores.UnitTests;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class EventHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task HandleIsOnlyCalledOnce()
        {
            var dateTimeOffset = DateTimeOffset.Now;
            var domainEventWrapper = new DomainEventWrapper
            {
                Created = dateTimeOffset,
                DomainEvent = new TestEv2()
            };

            var handleAsync = new Handler1();
            var handleAsync2 = new Handler2();
            var eventDelegateHandler = new AsyncEventHandler<TestEv2>(
                new VersionRepository(MicrowaveDatabase),
                new EventFeedMock(dateTimeOffset, domainEventWrapper),
                new List<IHandleAsync<TestEv2>> {handleAsync, handleAsync2});

            await eventDelegateHandler.Update();
            await eventDelegateHandler.Update();

            Assert.AreEqual(1, handleAsync.TimesCalled);
            Assert.AreEqual(1, handleAsync2.TimesCalled);
        }
    }

    public class EventFeedMock : IEventFeed<AsyncEventHandler<TestEv2>>
    {
        private readonly DateTimeOffset _dateTimeOffset;
        private readonly DomainEventWrapper _domainEventWrapper;

        public EventFeedMock(DateTimeOffset dateTimeOffset, DomainEventWrapper domainEventWrapper)
        {
            _dateTimeOffset = dateTimeOffset;
            _domainEventWrapper = domainEventWrapper;
        }

        #pragma warning disable 1998
        public async Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
        #pragma warning restore 1998
        {
            if (since < _dateTimeOffset)
                return new List<DomainEventWrapper> {_domainEventWrapper};
            return new List<DomainEventWrapper>();
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
        public Identity EntityId { get; }
    }

    public class TestEv2 : IDomainEvent
    {
        public Identity EntityId { get; }
    }
}