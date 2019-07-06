using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.ServiceMaps;
using Microwave.UI.Areas.MicrowaveDashboard.Pages;
using Moq;

namespace Microwave.UI.UnitTests
{
    [TestClass]
    public class ServicePageMapTests
    {
        [TestMethod]
        public void GetNodesAndEdges()
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
                    new ServiceEndPoint(new Uri("http://1234.de"), "MostFrequentService"),
                    new List<ServiceEndPoint>(),
                    true)
            });
            discoMock.Setup(m => m.GetServiceMap()).ReturnsAsync(map);
            var serviceMapPage = new ServiceMapPage(discoMock.Object, null);

            serviceMapPage.OnGetAsync().Wait();
            var nodes = serviceMapPage.OrderedNodes.ToList();

            Assert.AreEqual(3, nodes.Count);
            Assert.AreEqual("http://1234.de/", nodes[0].serviceAddress);
            Assert.AreEqual("MostFrequentService", nodes[0].label);
            Assert.AreEqual("http://1234.de/", nodes[0].id);
            Assert.AreEqual(0, nodes[0].x);
            Assert.AreEqual(0, nodes[0].y);
            Assert.AreEqual(new Uri("http://123.de"), nodes[1].serviceAddress);
            Assert.AreEqual(new Uri("http://12.de"), nodes[2].serviceAddress);
            Assert.AreEqual(0, nodes[2].x);
            Assert.AreEqual(1, nodes[2].y);

            var edges = serviceMapPage.Edges.ToList();
            Assert.AreEqual(4, edges.Count);
            Assert.AreEqual("http://12.de/", edges[0].source);
            Assert.AreEqual("http://123.de/", edges[0].target);
            Assert.AreEqual("http://12.de/", edges[1].source);
            Assert.AreEqual("http://1234.de/", edges[1].target);
            Assert.AreEqual("http://123.de/", edges[2].source);
            Assert.AreEqual("http://12.de/", edges[2].target);
            Assert.AreEqual("http://123.de/", edges[3].source);
            Assert.AreEqual("http://1234.de/", edges[3].target);
        }
    }
}