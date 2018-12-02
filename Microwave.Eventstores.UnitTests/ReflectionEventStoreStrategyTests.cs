using System;
using System.Collections.Generic;
using Microwave.Domain;
using Microwave.EventStores;
using NUnit.Framework;

namespace Microwave.Eventstores.UnitTests
{
    public class ReflectionEventStoreStrategyTests
    {
        [Test]
        public void LoadEntity()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<IDomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestChangeNameReflectionEvent(entityId, "NewName")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestReflectionEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.AreEqual("NewName", testEntity.Name);
            Assert.AreEqual(entityId, testEntity.Id);
        }

        [Test]
        public void LoadEntity_OldUnusedPropIsCalled()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<IDomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestCreatedReflectionEventOldLastNameEvent(entityId, "Old entity Name")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestReflectionEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.AreEqual("OldName", testEntity.Name);
            Assert.AreEqual(entityId, testEntity.Id);
        }

        [Test]
        public void LoadEntity_CascadingProp()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<IDomainEvent> { new TestCreatedNestedEvent(entityId, "OldName", new Adress("OldStreet", 12)), new TestStreetChangedEvent(entityId, "New Street Name")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.AreEqual("OldName", testEntity.Name);
            Assert.AreEqual(entityId, testEntity.Id);
            Assert.AreEqual("New Street Name", testEntity.Adress.Street);
            Assert.AreEqual(12, testEntity.Adress.Number);
        }

        [Test]
        public void LoadEntity_OverridingPropertyByAccident()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<IDomainEvent> { new TestCreatedNestedEvent(entityId, "OldName", new Adress("OldStreet", 12)), new TestStreetChangedEventWithError(entityId, "NewName of street", 15)};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.AreEqual("NewName of street", testEntity.Name);
            Assert.AreEqual(entityId, testEntity.Id);
            Assert.AreEqual("OldStreet", testEntity.Adress.Street);
            Assert.AreEqual(15, testEntity.Adress.Number);
        }

        [Test]
        public void  LoadEntity_OverridingProperty_ThatStillFillsOtherProperty()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<IDomainEvent> { new TestCreatedReflectionEvent(entityId, "ThePreName"), new TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName(entityId, "TheLastName")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.AreEqual("TheLastName", testEntity.LastName);
            Assert.AreEqual("ThePreName", testEntity.Name);
            Assert.AreEqual(entityId, testEntity.Id);
        }
    }

    internal class TestChangeNameReflectionEvent: IDomainEvent
    {
        [ActualPropertyName("Name")]
        public string NewName { get; }

        public TestChangeNameReflectionEvent(Guid entityId, string newName)
        {
            EntityId = entityId;
            NewName = newName;
        }

        public Guid EntityId { get; }
    }

    internal class TestCreatedReflectionEvent: IDomainEvent
    {
        public string Name { get; }

        public TestCreatedReflectionEvent(Guid entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestCreatedReflectionEventOldLastNameEvent: IDomainEvent
    {
        public string LastName { get; }

        public TestCreatedReflectionEventOldLastNameEvent(Guid entityId, string name)
        {
            EntityId = entityId;
            LastName = name;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName: IDomainEvent
    {
        [ActualPropertyName("LastName")]
        public string Name { get; }

        public TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName(Guid entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestCreatedNestedEvent: IDomainEvent
    {
        public string Name { get; }
        public Adress Adress { get; }

        public TestCreatedNestedEvent(Guid entityId, string name, Adress adress)
        {
            EntityId = entityId;
            Name = name;
            Adress = adress;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestStreetChangedEvent: IDomainEvent
    {
        [ActualPropertyName("Adress.Street")]
        public string Street { get; }

        public TestStreetChangedEvent(Guid entityId, string street)
        {
            EntityId = entityId;
            Street = street;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestStreetChangedEventWithError: IDomainEvent
    {
        public string Name { get; }

        [ActualPropertyName("Adress.Number")]
        public int Number { get; }

        public TestStreetChangedEventWithError(Guid entityId, string streetName, int number)
        {
            EntityId = entityId;
            Name = streetName;
            Number = number;
        }

        [ActualPropertyName("Id")]
        public Guid EntityId { get; }
    }

    internal class TestReflectionEntity
    {
        public string Name { get; private set; }
        public Guid Id { get; set; }
    }

    internal class TestNestedEntity
    {
        public TestNestedEntity(string name, Adress adress)
        {
            Name = name;
            Adress = adress;
        }

        public TestNestedEntity()
        {
        }

        public string Name { get; private set; }
        public string LastName { get; private set;  }
        public Adress Adress { get; private set;  }
        public Guid Id { get; set; }
    }

    internal class Adress
    {
        public Adress(string street, int number)
        {
            Street = street;
            Number = number;
        }

        public string Street { get; private set;  }
        public int Number { get; private set;  }
    }
}