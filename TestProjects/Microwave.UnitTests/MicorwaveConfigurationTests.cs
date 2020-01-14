using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Queries.Ports;

namespace Microwave.UnitTests
{
    [TestClass]
    public class MicorwaveConfigurationTests
    {
        [TestMethod]
        public void CanNotInstantiateWithWrongType()
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            Assert.ThrowsException<ProvidedTypeIsNoEventFeedException>(() => microwaveConfiguration.WithFeedType(typeof(Entity)));
        }

        [TestMethod]
        public void CanNotInstantiateWithInterface()
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            Assert.ThrowsException<ProvidedTypeIsNoEventFeedException>(() => microwaveConfiguration.WithFeedType(typeof(IEventFeed<>)));
        }

        [TestMethod]
        public void CanNotInstantiateWithConcreteInterface()
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            Assert.ThrowsException<ProvidedTypeIsNoEventFeedException>(() => microwaveConfiguration.WithFeedType(typeof
                (IEventFeed<Entity>)));
        }

        [TestMethod]
        public void CanInstantiateWithCorrectType()
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            microwaveConfiguration.WithFeedType(typeof(LocalEventFeed<>));
            Assert.AreEqual(typeof(LocalEventFeed<>), microwaveConfiguration.FeedType);
        }
    }
}