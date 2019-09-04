using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Subscriptions.Ports;
using Microwave.Subscriptions.ReadModels;
using Moq;

namespace Microwave.Subscriptions.UnitTests
{
    [TestClass]
    public class SubscriptionHandlerTests
    {
        [TestMethod]
        public async Task SubscribeOnServices()
        {
            var mock = new Mock<IRemoteSubscriptionRepository>();

            var discoveryHandler = new SubscriptionHandler(
                new EventsSubscribedByServiceReadModel(),
                null,
                mock.Object,
                null);

            await discoveryHandler.SubscribeOnDiscoveredServices();

           // mock.Verify(m => m.SubscribeForEvent(It.IsAny<Subscription>()), Times.Exactly(2));
        }
    }
}