using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Identities;
using Microwave.Persistence.MongoDb.UnitTestsSetup;
using Microwave.Queries.Persistence.MongoDb;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class EventHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task HandleIsOnlyCalledOnce()
        {
            var dateTimeOffset = DateTimeOffset.Now;
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Created = dateTimeOffset,
                DomainEvent = new TestEv2(GuidIdentity.Create())
            };

            var handleAsync = new Handler1();
            var handleAsync2 = new Handler2();
            var eventDelegateHandler = new AsyncEventHandler<TestEv2>(
                new VersionRepository(EventDatabase),
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
        private readonly SubscribedDomainEventWrapper _domainEventWrapper;

        public EventFeedMock(DateTimeOffset dateTimeOffset, SubscribedDomainEventWrapper domainEventWrapper)
        {
            _dateTimeOffset = dateTimeOffset;
            _domainEventWrapper = domainEventWrapper;
        }

        #pragma warning disable 1998
        public async Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
        #pragma warning restore 1998
        {
            if (since < _dateTimeOffset)
                return new List<SubscribedDomainEventWrapper> {_domainEventWrapper};
            return new List<SubscribedDomainEventWrapper>();
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
}