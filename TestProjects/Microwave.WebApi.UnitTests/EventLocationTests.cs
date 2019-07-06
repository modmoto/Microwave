using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class EventLocationTests
    {
        [TestMethod]
        public void AddFeedDoesReplaceOldOne()
        {
            var eventLocation = new EventLocation(
                new List<EventsPublishedByService>
                {
                    EventsPublishedByService.Reachable(
                        new ServiceEndPoint(new Uri("http://jeah.de")), new []
                        {
                            new EventSchema("Event2")
                        })
                },
                new EventsSubscribedByService(
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
                new List<EventsPublishedByService>
                {
                    EventsPublishedByService.Reachable(
                        new ServiceEndPoint(new Uri("http://jeah.de")), new []
                        {
                            new EventSchema("Event2",
                                new []{ new PropertyType("VorName", "String"),
                                        new PropertyType("LastName", "Int")
                                    })
                        })
                },
                new EventsSubscribedByService(
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
                new List<EventsPublishedByService>
                {
                    EventsPublishedByService.Reachable(
                        new ServiceEndPoint(new Uri("http://jeah.de")), new []
                        {
                            new EventSchema("Event2",
                                new []{ new PropertyType("VorName", "String"),
                                    new PropertyType("LastName", "Int")
                                })
                        })
                },
                new EventsSubscribedByService(
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

        [TestMethod]
        public void ReadmodelProperties_PropertyNotFound()
        {
            var eventLocation = new EventLocation(
                new List<EventsPublishedByService>
                {
                    EventsPublishedByService.Reachable(
                        new ServiceEndPoint(new Uri("http://jeah.de")), new []
                        {
                            new EventSchema("Event2",
                                new []{ new PropertyType("VorName", "String"),
                                    new PropertyType("LastName", "Int")
                                })
                        })
                },
                new EventsSubscribedByService(
                    new List<EventSchema>(),
                    new []{ new ReadModelSubscription("ReadModel2", new EventSchema("Event2",
                        new []
                        {
                            new PropertyType("VorNameNotInService", "String"),
                            new PropertyType("VorName", "String")
                        }))}));

            var service = eventLocation.GetServiceForReadModel(typeof(ReadModel2));

            Assert.AreEqual(nameof(ReadModel2), service.ReadModels.Single().ReadModelName);
            var getsCreatedOn = service.ReadModels.Single().GetsCreatedOn;
            var propertyTypes = getsCreatedOn.Properties.ToList();
            Assert.AreEqual(2, propertyTypes.Count);
            Assert.AreEqual(nameof(Event2), getsCreatedOn.Name);

            Assert.AreEqual("VorNameNotInService", propertyTypes[0].Name);
            Assert.AreEqual("String", propertyTypes[0].Type);
            Assert.IsFalse(propertyTypes[0].IsPresentInRemote);

            Assert.AreEqual("VorName", propertyTypes[1].Name);
            Assert.AreEqual("String", propertyTypes[1].Type);
            Assert.IsTrue(propertyTypes[1].IsPresentInRemote);
        }
    }

    public class ReadModel2
    {
    }

    public class Event2
    {
    }

    public class Event1
    {
    }
}