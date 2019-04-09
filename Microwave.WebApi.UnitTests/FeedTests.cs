using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.WebApi.Querries;
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
            mockHttp.When("http://localost:5000/api/DomainEvents/?timeStamp=0001-01-01T00:00:00.0000000+00:00")
                .Respond("application/json", "[{ \"domainEventType\": \"UNKNOWN_TYPE\",\"version\": 12, \"created\": \"2018-01-07T19:07:21.227631+01:00\", \"domainEvent\": {\"EntityId\" : \"5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180\"}}, { \"domainEventType\": \"TestEv\",\"version\": 12, \"created\": \"2018-01-07T19:07:31.227631+01:00\", \"domainEvent\": {\"EntityId\" : \"5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180\" }}]");

            var domainOverallEventClient = new DomainEventClient<ReadModelHandler<TestReadModel>>(mockHttp);
            domainOverallEventClient.BaseAddress = new Uri("http://localost:5000/api/DomainEvents/");

            var domainEventFactory = new DomainEventFactory(_eventTypeRegistration);
            var domainEventWrapperListDeserializer = new DomainEventWrapperListDeserializer(new JSonHack(), domainEventFactory);
            var readModelFeed = new EventFeed<ReadModelHandler<TestReadModel>>(domainEventWrapperListDeserializer, domainOverallEventClient);
            var domainEvents = await readModelFeed.GetEventsAsync();
            var domainEventWrappers = domainEvents.ToList();
            Assert.AreEqual(1, domainEventWrappers.Count);
            Assert.AreEqual(new Guid("5a8b63c8-0f7f-4de7-a9e5-b6b377aa2180").ToString(), domainEventWrappers[0].DomainEvent
                .EntityId.Id);
        }

        [TestMethod]
        public async Task ReadModelFeed_Exception()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localost:5000/api/DomainEvents/?timeStamp=0001-01-01T00:00:00.0000000+00:00")
                .Throw(new HttpRequestException());
            var domainOverallEventClient = new DomainEventClient<ReadModelHandler<TestReadModel>>(mockHttp);
            domainOverallEventClient.BaseAddress = new Uri("http://localost:5000/api/DomainEvents/");

            var domainEventFactory = new DomainEventFactory(_eventTypeRegistration);
            var domainEventWrapperListDeserializer = new DomainEventWrapperListDeserializer(new JSonHack(), domainEventFactory);
            var readModelFeed = new EventFeed<ReadModelHandler<TestReadModel>>(domainEventWrapperListDeserializer, domainOverallEventClient);
            var domainEvents = await readModelFeed.GetEventsAsync();
            var domainEventWrappers = domainEvents.ToList();

            Assert.AreEqual(0, domainEventWrappers.Count);
        }
    }

    public class TestReadModel : ReadModel
    {
        public void Handle(TestEv ev)
        {
            Id = ev.EntityId;
        }

        public Identity Id { get; set; }
        public override Type GetsCreatedOn { get; }
    }

    public class TestEv : IDomainEvent
    {
        public TestEv(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}