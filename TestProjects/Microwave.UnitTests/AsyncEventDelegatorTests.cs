using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;

namespace Microwave.UnitTests
{
    [TestClass]
    public class AsyncEventDelegatorTests
    {
        [TestMethod] public void RunMockWithoutAttribute_AsyncHandler()
        {
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler> {new AsyncHandlerMock()}, new
                List<IQueryEventHandler>(), new List<IReadModelEventHandler>(), null, new List<IPollingInterval>
            {
                new PollingInterval<AsyncHandlerImplemented>(3)
            });

            asyncEventDelegator.StartEventPolling();
        }

        [TestMethod]
        public void RunMockWithoutAttribute_ReadModelHandler()
        {
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler>(), new
                List<IQueryEventHandler>(), new List<IReadModelEventHandler> { new RmHandlerMock() }, null, new
                List<IPollingInterval>
            {
                new PollingInterval<AsyncHandlerImplemented>(3)
            });

            asyncEventDelegator.StartEventPolling();
        }

        [TestMethod]
        public void RunMockWithoutAttribute_QueryHandler()
        {
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler>(), new
                List<IQueryEventHandler> { new QHandlerMock() }, new List<IReadModelEventHandler>(), null, new
                List<IPollingInterval>
            {
                new PollingInterval<AsyncHandlerImplemented>(3)
            });

            asyncEventDelegator.StartEventPolling();
        }

        [TestMethod]
        public void RunMockWithAttribute()
        {
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler>
            {
                new
                    AsyncHandlerMockWithoutAttribute()
            }, new
                List<IQueryEventHandler>(), new List<IReadModelEventHandler>());

            asyncEventDelegator.StartEventPolling();
        }
    }

    public class RmHandlerMock : IReadModelEventHandler
    {
        public Task Update()
        {
            return Task.CompletedTask;
        }
    }

    public class QHandlerMock : IQueryEventHandler
    {
        public Task Update()
        {
            return Task.CompletedTask;
        }
    }

    public class AsyncHandlerMockWithoutAttribute : IAsyncEventHandler
    {
        public Task Update()
        {
            return Task.CompletedTask;
        }

        public Type HandlerClassType => typeof(AssemblyCleanupAttribute);
    }

    public class AsyncHandlerMock : IAsyncEventHandler
    {
        public Task Update()
        {
            return Task.CompletedTask;
        }

        public Type HandlerClassType => typeof(AsyncHandlerImplemented);
    }

    public class AsyncHandlerImplemented
    {
    }
}