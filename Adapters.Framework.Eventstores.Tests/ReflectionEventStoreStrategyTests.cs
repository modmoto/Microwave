using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Moq;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class ReflectionEventStoreStrategyTests
    {
        [Fact]
        public void LoadEntity()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestChangeNameReflectionEvent(entityId, "NewName")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestReflectionEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.Equal("NewName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }

        [Fact]
        public void LoadEntity_OldUnusedPropIsCalled()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestCreatedReflectionEventOldLastNameEvent(entityId, "Old entity Name")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestReflectionEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.Equal("OldName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }

        [Fact]
        public void LoadEntity_CascadingProp()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedNestedEvent(entityId, "OldName", new Adress("OldStreet", 12)), new TestStreetChangedEvent(entityId, "New Street Name")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.Equal("OldName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
            Assert.Equal("New Street Name", testEntity.Adress.Street);
            Assert.Equal(12, testEntity.Adress.Number);
        }

        [Fact]
        public void LoadEntity_OverridingPropertyByAccident()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedNestedEvent(entityId, "OldName", new Adress("OldStreet", 12)), new TestStreetChangedEventWithError(entityId, "NewName of street", 15)};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.Equal("NewName of street", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
            Assert.Equal("OldStreet", testEntity.Adress.Street);
            Assert.Equal(15, testEntity.Adress.Number);
        }

        [Fact]
        public void  LoadEntity_OverridingProperty_ThatStillFillsOtherProperty()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "ThePreName"), new TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName(entityId, "TheLastName")};

            var eventSourcingApplyStrategy = new EventSourcingAtributeStrategy();
            var testEntity = new TestNestedEntity();
            foreach (var domainEvent in domainEvents)
            {
                testEntity = eventSourcingApplyStrategy.Apply(testEntity, domainEvent);
            }

            Assert.Equal("TheLastName", testEntity.LastName);
            Assert.Equal("ThePreName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }
    }

    internal class TestChangeNameReflectionEvent : DomainEvent
    {
        [ActualPropertyName("Name")]
        public string NewName { get; }

        public TestChangeNameReflectionEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }
    }

    internal class TestCreatedReflectionEvent : DomainEvent
    {
        public string Name { get; }

        public TestCreatedReflectionEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }

    internal class TestCreatedReflectionEventOldLastNameEvent : DomainEvent
    {
        public string LastName { get; }

        public TestCreatedReflectionEventOldLastNameEvent(Guid entityId, string name) : base(entityId)
        {
            LastName = name;
        }
    }

    internal class TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName : DomainEvent
    {
        [ActualPropertyName("LastName")]
        public string Name { get; }

        public TestCreatedReflectionEventLastNameEventWithAccidentlyOverridingOtherName(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }

    internal class TestCreatedNestedEvent : DomainEvent
    {
        public string Name { get; }
        public Adress Adress { get; }

        public TestCreatedNestedEvent(Guid entityId, string name, Adress adress) : base(entityId)
        {
            Name = name;
            Adress = adress;
        }
    }

    internal class TestStreetChangedEvent : DomainEvent
    {
        [ActualPropertyName("Adress.Street")]
        public string Street { get; }

        public TestStreetChangedEvent(Guid entityId, string street) : base(entityId)
        {
            Street = street;
        }
    }

    internal class TestStreetChangedEventWithError : DomainEvent
    {
        public string Name { get; }

        [ActualPropertyName("Adress.Number")]
        public int Number { get; }

        public TestStreetChangedEventWithError(Guid entityId, string streetName, int number) : base(entityId)
        {
            Name = streetName;
            Number = number;
        }
    }

    internal class TestReflectionEntity : Entity
    {
        public string Name { get; private set; }
    }

    internal class TestNestedEntity : Entity
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