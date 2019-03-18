using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Discovery;
using Microwave.Eventstores.UnitTests;
using Moq;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class DiscoveryHandlerTests : IntegrationTests
    {
        [TestMethod]
        [Ignore]
        public async Task HandlerCallsServicesAndSavesResults()
        {
            var discoveryRepo = new Mock<IServiceDiscoveryRepository>();
            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://Service1.de"))).ReturnsAsync(new
                ConsumingService(new Uri("http://Service1.de"), new[] {"Event1"}));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://Service2.de"))).ReturnsAsync(new
                ConsumingService(new Uri("http://Service2.de"), new[] {"Event2"}));
            var discoveryHandler = new DiscoveryHandler(new ServiceBaseAdressCollection
            {
                new Uri("http://Service1.de"),
                new Uri("http://Service2.de"),
                new Uri("http://Service3.de")
            },
                new SubscribedEventCollection(
                    new []{ "Event1"},
                    new []{ new ReadModelSubscription(new []{"Event2"}, "Event2") }),
                discoveryRepo.Object);

            var consumingServices = (await discoveryHandler.GetConsumingServices()).ToList();

            Assert.AreEqual(2, consumingServices.Count);
            Assert.AreEqual("Service1", consumingServices[0].ServiceName);
            Assert.AreEqual("Service2", consumingServices[1].ServiceName);
            Assert.AreEqual("http://Service1.de", consumingServices[0].ServiceBaseAddress);
            Assert.AreEqual("http://Service1.de", consumingServices[1].ServiceBaseAddress);
        }
    }
}