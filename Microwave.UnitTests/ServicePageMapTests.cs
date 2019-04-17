using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.Domain.Services;
using Microwave.Pages;
using Moq;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ServicePageMapTests
    {
        [TestMethod]
        public void CalculateServicesByIncomingNodes()
        {
            var discoMock = new Mock<IDiscoveryHandler>();
            var map = new ServiceMap(new List<ServiceNodeConfig>
            {
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://12.de"), "Name"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://123.de"), "Name"),
                        new ServiceEndPoint(new Uri("http://1234.de"), "Name")
                    },
                    true),
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://123.de"), "Name2"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://12.de"), "Name2"),
                        new ServiceEndPoint(new Uri("http://1234.de"), "Name")
                    },
                    true),
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://1234.de")),
                    new List<ServiceEndPoint>(),
                    true)
            });
            discoMock.Setup(m => m.GetServiceMap()).ReturnsAsync(map);
            var serviceMapPage = new ServiceMapPage(discoMock.Object);

            serviceMapPage.OnGetAsync().Wait();
            var servicesSortedByIncomingNodes = serviceMapPage.ServicesSortedByIncomingNodes.ToList();

            Assert.AreEqual(3, servicesSortedByIncomingNodes.Count);
            Assert.AreEqual(new Uri("http://1234.de"), servicesSortedByIncomingNodes[0].ServiceEndPoint.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://123.de"), servicesSortedByIncomingNodes[1].ServiceEndPoint.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://12.de"), servicesSortedByIncomingNodes[2].ServiceEndPoint.ServiceBaseAddress);
        }
    }
}