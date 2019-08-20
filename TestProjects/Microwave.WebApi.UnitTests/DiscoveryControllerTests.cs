using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.WebApi.Discovery;
using Moq;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DiscoveryControllerTests
    {
        [TestMethod]
        public async Task GetPublishedEvents()
        {
            var mock = new Mock<IDiscoveryHandler>();
            var eventsPublishedByService = EventsPublishedByService.Reachable(
                new ServiceEndPoint(new Uri("http://123.de"), "Name"),
                new List<EventSchema>());
            mock.Setup(m => m.GetPublishedEvents()).ReturnsAsync(eventsPublishedByService);
            var discoveryController = new DiscoveryController(mock.Object, null);

            await discoveryController.GetPublishedEvents();

            var result = await discoveryController.GetPublishedEvents();

            var viewResult = result as OkObjectResult;
            var publishedEventsByServiceDto = viewResult.Value as PublishedEventsByServiceDto;

            Assert.AreEqual(0, publishedEventsByServiceDto.PublishedEvents.Count);
            Assert.AreEqual("Name", publishedEventsByServiceDto.ServiceName);
        }
    }
}