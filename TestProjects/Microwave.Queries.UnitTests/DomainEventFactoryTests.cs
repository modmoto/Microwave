using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.WebApi.Queries;
using Newtonsoft.Json;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class DomainEventFactoryTests
    {
        [TestMethod]
        public void ParseDomainEventWrapper()
        {
            var domainEventWrapper = new [] { new DomainEventWrapper
            {
                EntityStreamVersion = 12,
                OverallVersion = 1,
                DomainEvent = new Event1(Guid.NewGuid(), "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration { { nameof(Event1), typeof(Event1) } };
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var ev = domainEventFactory.Deserialize(serializeObject);
            var domainEventWrappers = ev.ToList();
            var wrapperActual = domainEventWrappers.Single();
            var wrapperExpected = domainEventWrapper.Single();
            Assert.AreEqual(wrapperExpected.DomainEvent.EntityId, wrapperActual.DomainEvent.EntityId);
            Assert.AreEqual(wrapperExpected.EntityStreamVersion, wrapperActual.EntityStreamVersion);
            Assert.AreEqual(wrapperExpected.OverallVersion, wrapperActual.OverallVersion);
            Assert.AreEqual(((Event1) wrapperExpected.DomainEvent).Name, ((Event1)wrapperActual.DomainEvent).Name);
        }

        [TestMethod]
        public void ParseDomainEventWrapper_StringKey()
        {
            var domainEventWrapper = new [] { new DomainEventWrapper
            {
                EntityStreamVersion = 12,
                OverallVersion = 0,
                DomainEvent = new Event2("luls", "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration { { nameof(Event2), typeof(Event2) } };
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var domainEventWrappers = domainEventFactory.Deserialize(serializeObject).ToList();
            var wrapperActual = domainEventWrappers.Single();

            Assert.AreEqual("luls", wrapperActual.DomainEvent.EntityId);
        }

        [TestMethod]
        public void ParseDomainEventWrapper_NoKeyInRegistration()
        {
            var domainEventWrapper = new [] { new DomainEventWrapper
            {
                EntityStreamVersion = 12,
                OverallVersion = 0,
                DomainEvent = new Event1(Guid.NewGuid(), "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration();
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var ev = domainEventFactory.Deserialize(serializeObject);
            var domainEventWrappers = ev.ToList();
            Assert.AreEqual(0, domainEventWrappers.Count);
        }
    }

    public class Event1 : IDomainEvent, ISubscribedDomainEvent
    {
        public string EntityId { get; }
        public string Name { get; }

        public Event1(Guid entityId, string name)
        {
            EntityId = entityId.ToString();
            Name = name;
        }
    }

    public class Event2 : IDomainEvent, ISubscribedDomainEvent
    {
        public string EntityId { get; }
        public string Name { get; }

        public Event2(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}