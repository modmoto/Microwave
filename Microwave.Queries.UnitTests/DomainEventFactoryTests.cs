using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Domain;
using Microwave.ObjectPersistences;
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
                Version = 12,
                Created = 1234,
                DomainEvent = new Event1(GuidIdentity.Create(Guid.NewGuid()), "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration { { nameof(Event1), typeof(Event1) } };
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var ev = domainEventFactory.Deserialize(serializeObject);
            var domainEventWrappers = ev.ToList();
            var wrapperActual = domainEventWrappers.Single();
            var wrapperExpected = domainEventWrapper.Single();
            Assert.AreEqual(wrapperExpected.DomainEvent.EntityId.Id, wrapperActual.DomainEvent.EntityId.Id);
            Assert.AreEqual(wrapperExpected.Version, wrapperActual.Version);
            Assert.AreEqual(wrapperExpected.Created, wrapperActual.Created);
            Assert.AreEqual(((Event1) wrapperExpected.DomainEvent).Name, ((Event1)wrapperActual.DomainEvent).Name);
        }

        [TestMethod]
        public void ParseDomainEventWrapper_StringKey()
        {
            var domainEventWrapper = new [] { new DomainEventWrapper
            {
                Version = 12,
                Created = 1234,
                DomainEvent = new Event2(StringIdentity.Create("luls"), "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration { { nameof(Event2), typeof(Event2) } };
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var domainEventWrappers = domainEventFactory.Deserialize(serializeObject).ToList();
            var wrapperActual = domainEventWrappers.Single();

            Assert.AreEqual("luls", wrapperActual.DomainEvent.EntityId.Id);
        }

        [TestMethod]
        public void ParseDomainEventWrapper_NoKeyInRegistration()
        {
            var domainEventWrapper = new [] { new DomainEventWrapper
            {
                Version = 12,
                Created = 1234,
                DomainEvent = new Event1(GuidIdentity.Create(Guid.NewGuid()), "Name")
            }};

            var serializeObject = JsonConvert.SerializeObject(domainEventWrapper);
            var eventTypeRegistration = new EventRegistration();
            var domainEventFactory = new DomainEventFactory(eventTypeRegistration);
            var ev = domainEventFactory.Deserialize(serializeObject);
            var domainEventWrappers = ev.ToList();
            Assert.AreEqual(0, domainEventWrappers.Count);
        }
    }

    public class Event1 : IDomainEvent
    {
        public Identity EntityId { get; }
        public string Name { get; }

        public Event1(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }

    public class Event2 : IDomainEvent
    {
        public Identity EntityId { get; }
        public string Name { get; }

        public Event2(StringIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}