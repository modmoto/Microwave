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
            var eventLocation = new EventLocation();
            var service = new SubscriberEventAndReadmodelConfig(new Uri("http://localhost:5000"), new List<string> {
            "Event1"}, new []{ new ReadModelSubscription("ReadModel1", "Event1")  }, "TeamService" );
            var service2 = new SubscriberEventAndReadmodelConfig(new Uri("http://localhost:5000"), new List<string> {
                "Event2"}, new []{ new ReadModelSubscription("ReadModel2", "Event1")  }, "TeamService" );

            eventLocation.SetDomainEventLocation(service);
            eventLocation.SetDomainEventLocation(service2);

            var serviceAfter2 = eventLocation.GetServiceForEvent(typeof(Event2));
            var serviceAfter1 = eventLocation.GetServiceForEvent(typeof(Event1));

            Assert.AreEqual(nameof(Event2), serviceAfter2.SubscribedEvents.Single());
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