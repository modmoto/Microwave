using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.UI.Areas.MicrowaveDashboard.Pages;
using Microwave.UnitTests;

namespace Microwave.UI.UnitTests
{
    [TestClass]
    public class JobPagesTests
    {
        [TestMethod]
        public void JobdtoConstructor()
        {
            var jobDto = new JobDto(new MicrowaveBackgroundService<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>(null), 1);

            Assert.AreEqual(jobDto.Index, 1);
            Assert.AreEqual(jobDto.EventName, nameof(TestDomainEvent1));
            Assert.AreEqual(jobDto.HandlerName, nameof(TestEventHandler));
            Assert.AreEqual(jobDto.GenericType, "AsyncEventHandler");
        }
    }
}