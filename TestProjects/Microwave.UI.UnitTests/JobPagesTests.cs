using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.UI.Areas.MicrowaveDashboard.Pages;
using Microwave.UnitTests;
using Microwave.WebApi;

namespace Microwave.UI.UnitTests
{
    [TestClass]
    public class JobPagesTests
    {
        [TestMethod]
        public void JobdtoConstructor()
        {
            var jobDto = new JobDto(new MicrowaveBackgroundService<AsyncEventHandler<TestEventHandler,
            TestDomainEvent1>>(null, new PollingInterval<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>()), 1);

            Assert.AreEqual(jobDto.Index, 1);
            Assert.AreEqual(jobDto.EventName, nameof(TestDomainEvent1));
            Assert.AreEqual(jobDto.HandlerName, nameof(TestEventHandler));
            Assert.AreEqual(jobDto.GenericType, "AsyncEventHandler");
            Assert.IsNotNull(jobDto.NextRun);
        }

        [TestMethod]
        public void JobsPage()
        {
            var mock1 = new MicrowaveBackgroundService<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>(null,
            new PollingInterval<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>());
            var mock2 = new MicrowaveBackgroundService<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>(null,
            new PollingInterval<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>());
            var jobsPage = new JobsPage(
                new MicrowaveWebApiConfiguration(),
                new List<IMicrowaveBackgroundService> { mock1, mock2 });

            Assert.IsNotNull(jobsPage.Jobs);

            var onPostAsync = jobsPage.OnPostAsync(1);
            Assert.IsNotNull(onPostAsync);
            Assert.AreEqual(2, jobsPage.Jobs.Count());
        }

        [TestMethod]
        public void JobsPage_DiscoveryPollerIsFilteredOut()
        {
            var mock1 = new MicrowaveBackgroundService<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>(null,
            new PollingInterval<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>());
            var mock2 = new MicrowaveBackgroundService<DiscoveryPoller>(null, new PollingInterval<DiscoveryPoller>());
            var jobsPage = new JobsPage(
                new MicrowaveWebApiConfiguration(),
                new List<IMicrowaveBackgroundService> { mock1, mock2 });

            Assert.AreEqual(1, jobsPage.Jobs.Count());
        }
    }
}