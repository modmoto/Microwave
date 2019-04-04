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
                            new EventSchema("Event2")
                        })
                },
                new SubscribedEventCollection(
                    new List<EventSchema> { new EventSchema("Event2")},
                    new []{ new ReadModelSubscription("ReadModel2", new EventSchema("Event1"))}));

            var serviceAfter2 = eventLocation.GetServiceForEvent(typeof(Event2));
            var serviceAfter1 = eventLocation.GetServiceForEvent(typeof(Event1));

            Assert.AreEqual(nameof(Event2), serviceAfter2.SubscribedEvents.Single().Name);
            Assert.IsNull(serviceAfter1);
        }

        [TestMethod]
        public void SubscribedEventPropertiesArePropertiesOfConsumingService()
        {
            var eventLocation = new EventLocation(
                new List<PublisherEventConfig>
                {
                    new PublisherEventConfig(
                        new Uri("http://jeah.de"), new []
                        {
                            new EventSchema("Event2",
                                new []{ new PropertyType("VorName", "String"),
                                        new PropertyType("LastName", "Int")
                                    })
                        })
                },
                new SubscribedEventCollection(
                    new List<EventSchema> { new EventSchema("Event2", new []{ new PropertyType("VorName", "String"),  })},
                    new []{ new ReadModelSubscription("ReadModel2", new EventSchema("Event1"))}));

            var serviceAfter2 = eventLocation.GetServiceForEvent(typeof(Event2));

            Assert.AreEqual(nameof(Event2), serviceAfter2.SubscribedEvents.Single().Name);
            Assert.AreEqual("VorName", serviceAfter2.SubscribedEvents.Single().Properties.Single().Name);
            Assert.AreEqual("String", serviceAfter2.SubscribedEvents.Single().Properties.Single().Type);
        }

        [TestMethod]
        public void SubscribedEventProperties_PropertyNotFound()
        {
            var eventLocation = new EventLocation(
                new List<PublisherEventConfig>
                {
                    new PublisherEventConfig(
                        new Uri("http://jeah.de"), new []
                        {
                            new EventSchema("Event2",
                                new []{ new PropertyType("VorName", "String"),
                                    new PropertyType("LastName", "Int")
                                })
                        })
                },
                new SubscribedEventCollection(
                    new List<EventSchema> { new EventSchema("Event2", new []
                    {
                        new PropertyType("VorNameNotInService", "String"),
                        new PropertyType("VorName", "String")
                    })},
                    new []{ new ReadModelSubscription("ReadModel2", new EventSchema("Event1"))}));

            var serviceAfter2 = eventLocation.GetServiceForEvent(typeof(Event2));

            Assert.AreEqual(nameof(Event2), serviceAfter2.SubscribedEvents.Single().Name);
            var propertyTypes = serviceAfter2.SubscribedEvents.Single().Properties.ToList();
            Assert.AreEqual(2, propertyTypes.Count);

            Assert.AreEqual("VorNameNotInService", propertyTypes[0].Name);
            Assert.AreEqual("String", propertyTypes[0].Type);
            Assert.IsFalse(propertyTypes[0].IsPresentInRemote);

            Assert.AreEqual("VorName", propertyTypes[1].Name);
            Assert.AreEqual("String", propertyTypes[1].Type);
            Assert.IsTrue(propertyTypes[1].IsPresentInRemote);
        }
    }

    public class Event2
    {
    }

    public class Event1
    {
    }
}