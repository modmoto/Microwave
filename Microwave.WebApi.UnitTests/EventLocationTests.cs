using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Discovery;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class EventLocationTests
    {
        [TestMethod]
        public void AddFeedDoesReplaceOldOne()
        {
            var eventLocation = new EventLocation(
                new List<PublisherEventConfig>
                {
                    new PublisherEventConfig(
                        new Uri("http://jeah.de"), new []
                        {
                            new EventSchema { Name = "Event2"}
                        })
                },
                new SubscribedEventCollection(
                    new List<EventSchema> { new EventSchema { Name = "Event2"}},
                    new []{ new ReadModelSubscription("ReadModel2", new EventSchema { Name = "Event1"})}));

            var serviceAfter2 = eventLocation.GetServiceForEvent(typeof(Event2));
            var serviceAfter1 = eventLocation.GetServiceForEvent(typeof(Event1));

            Assert.AreEqual(nameof(Event2), serviceAfter2.SubscribedEvents.Single().Name);
            Assert.IsNull(serviceAfter1);
        }
    }

    public class Event2
    {
    }

    public class Event1
    {
    }
}