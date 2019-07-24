using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Moq;

namespace Microwave.UnitTests
{
    [TestClass]
    public class AsyncEventDelegatorTests
    {
        [TestMethod]
        public void RunMockWithoutAttribute()
        {
            var mock = new Mock<IDiscoveryHandler>();
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler> {new AsyncHandlerMock()}, new
                List<IQueryEventHandler>(), new List<IReadModelEventHandler>(), mock.Object);

            asyncEventDelegator.StartEventPolling();
        }

        [TestMethod]
        public void RunMockWithAttribute()
        {
            var mock = new Mock<IDiscoveryHandler>();
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler>
            {
                new
                    AsyncHandlerMockWithoutAttribute()
            }, new
                List<IQueryEventHandler>(), new List<IReadModelEventHandler>(), mock.Object);

            asyncEventDelegator.StartEventPolling();
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

    [UpdateEvery(3)]
    internal class AsyncHandlerImplemented
    {
    }
}