using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class AsyncEventDelegatorTests
    {
        [TestMethod]
        public async Task ExceptionIsThrown_IsRed()
        {
            var asyncEventHandler = new TestHandler2();
            var handlerList = new List<IAsyncEventHandler>
            {
                new TestHandler(),
                asyncEventHandler
            };
            var queryEventHandlers = new List<IQueryEventHandler> { new HandlerTest()};
            var identifiableQueryEventHandlers = new List<IReadModelHandler> { new IdHandlerTest()};
            var asyncEventDelegator = new AsyncEventDelegator(handlerList, queryEventHandlers, identifiableQueryEventHandlers);

            asyncEventDelegator.Update();

            await Task.Delay(2000);

            Assert.IsTrue(asyncEventHandler.WasCalled);
        }
    }

    public class IdHandlerTest : IReadModelHandler
    {
        public async Task Update()
        {
        }
    }

    public class HandlerTest : IQueryEventHandler
    {
        public async Task Update()
        {
        }
    }

    public class TestHandler : IAsyncEventHandler
    {
        public async Task Update()
        {
            throw new Exception("This is a test");
        }
    }

    public class TestHandler2 : IAsyncEventHandler
    {
        public async Task Update()
        {
            WasCalled = true;
        }

        public bool WasCalled { get; set; }
    }
}