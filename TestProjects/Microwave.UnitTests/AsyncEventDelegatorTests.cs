using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Queries.Handler;
using Moq;

namespace Microwave.UnitTests
{
    [TestClass]
    public class AsyncEventDelegatorTests
    {
        [TestMethod]
        public void AddDiContainerTest()
        {
            var mock = new Mock<IDiscoveryHandler>();
            var asyncEventDelegator = new AsyncEventDelegator(new List<IAsyncEventHandler>(), new
            List<IQueryEventHandler>(), new List<IReadModelEventHandler>(), mock.Object);

            asyncEventDelegator.StartEventPolling();
        }
    }
}