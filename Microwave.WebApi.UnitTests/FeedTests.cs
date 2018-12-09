using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.ObjectPersistences;
using Microwave.Queries;
using Moq;
using RichardSzalay.MockHttp;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class FeedTests
    {
        private readonly EventRegistration _eventTypeRegistration = new EventRegistration
        {
            { nameof(TestEv), typeof(TestEv)}
        };

        [TestMethod]
        public async Task ReadModelFeed()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localost:5000/api/DomainEvents/?timeStamp=0")
                .Respond("application/json", "[{ \"domainEventType\": \"UNKNOWN_TYPE\",\"version\": 12, \"created\": 14, \"domainEvent\": {\"EntityId\" : \"5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180\" }}, { \"domainEventType\": \"TestEv\",\"version\": 12, \"created\": 14, \"domainEvent\": {\"EntityId\" : \"5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180\" }}]");

            var domainOverallEventClient = new DomainOverallEventClient<TestReadModel>(mockHttp);
            domainOverallEventClient.BaseAddress = new Uri("http://localost:5000/api/DomainEvents/");

            var domainEventFactory = new DomainEventFactory(_eventTypeRegistration);
            var domainEventWrapperListDeserializer = new DomainEventWrapperListDeserializer(new JSonHack(), domainEventFactory);
            var readModelFeed = new ReadModelFeed<TestReadModel>(domainEventWrapperListDeserializer, domainOverallEventClient);
            var domainEvents = await readModelFeed.GetEventsAsync(0);
            var domainEventWrappers = domainEvents.ToList();
            Assert.AreEqual(1, domainEventWrappers.Count);
            Assert.AreEqual(new Guid("5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180"), domainEventWrappers[0].DomainEvent.EntityId);
        }
    }

    public class TestReadModel : ReadModel
    {
        public void Handle(TestEv ev)
        {
            Id = ev.EntityId;
        }

        public Guid Id { get; set; }
    }

    public class TestEv : IDomainEvent
    {
        public TestEv(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}