using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.UI.Areas.MicrowaveDashboard.Pages;
using Microwave.WebApi;
using Moq;

namespace Microwave.UI.UnitTests
{
    [TestClass]
    public class RestOfPagesTests
    {
        [TestMethod]
        public async Task GetConsumingServices()
        {
            var mock = new Mock<IDiscoveryHandler>();
            mock.Setup(m => m.GetConsumingServices()).ReturnsAsync(new EventLocation(
                new List<MicrowaveServiceNode>(),
                new List<EventSchema>(),
                new List<ReadModelSubscription>()));

            mock.Setup(m => m.GetPublishedEvents()).ReturnsAsync(EventsPublishedByService.Reachable(new
            ServiceEndPoint(null), new List<EventSchema>()));
            var indexModel = new IndexModel(mock.Object, new MicrowaveWebApiConfiguration());

            await indexModel.OnGetAsync();

            Assert.IsNotNull(indexModel.ConsumingServices);
        }

        [TestMethod]
        public async Task GetNodesAndEdges()
        {
            var mock = new Mock<IDiscoveryHandler>();
            mock.Setup(m => m.GetConsumingServices()).ReturnsAsync(new EventLocation(
                new List<MicrowaveServiceNode>(),
                new List<EventSchema>(),
                new List<ReadModelSubscription>()));

            mock.Setup(m => m.GetPublishedEvents()).ReturnsAsync(EventsPublishedByService.Reachable(new
                ServiceEndPoint(null), new List<EventSchema>()));

            var indexModel = new IndexModel(mock.Object, new MicrowaveWebApiConfiguration());

            await indexModel.OnGetAsync();

            Assert.IsFalse(indexModel.HasMissingEvents);
        }

        [TestMethod]
        public async Task Discover()
        {
            var mock = new Mock<IDiscoveryHandler>();
            var indexModel = new IndexModel(mock.Object, new MicrowaveWebApiConfiguration());

            await indexModel.OnPostAsync();

            mock.Verify(m => m.DiscoverConsumingServices(), Times.Once);
        }

        [TestMethod]
        public void DiscoveryOption()
        {
            var mock = new Mock<IWebHostEnvironment>();
            var fileProvider = new EmbeddedFileProvider(typeof(RestOfPagesTests).Assembly);
            mock.Setup(m => m.ContentRootFileProvider).Returns(fileProvider);
            var microwaveUiConfigureOptions = new MicrowaveUiConfigureOptions(mock.Object);

            var staticFileOptions = new StaticFileOptions();
            microwaveUiConfigureOptions.PostConfigure("hm", staticFileOptions);

            Assert.AreEqual(typeof(CompositeFileProvider), staticFileOptions.FileProvider.GetType());
        }

        [TestMethod]
        public void DiscoveryOptionThrowsException()
        {
            var mock = new Mock<IWebHostEnvironment>();
            var microwaveUiConfigureOptions = new MicrowaveUiConfigureOptions(mock.Object);

            Assert.ThrowsException<InvalidOperationException>(() =>
                microwaveUiConfigureOptions.PostConfigure("hm", new StaticFileOptions()));
        }
    }
}