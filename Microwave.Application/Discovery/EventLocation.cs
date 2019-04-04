using System;
using System.Collections.Generic;
using System.Linq;

namespace Microwave.Application.Discovery
{
    public class EventLocation : IEventLocation
    {
        public EventLocation(IEnumerable<PublisherEventConfig> allServices, SubscribedEventCollection subscribedEventCollection)
        {
            var readModels = subscribedEventCollection.ReadModelSubcriptions.ToList();
            var handleAsyncEvents = subscribedEventCollection.Events.ToList();
            UnresolvedEventSubscriptions = handleAsyncEvents;
            UnresolvedReadModeSubscriptions = readModels;

            foreach (var service in allServices)
            {
                var relevantEvents = new List<EventSchema>();
                var relevantEventsOfService = handleAsyncEvents.Where(ev =>
                    service.PublishedEventTypes.Contains(ev)).ToList();
                foreach (var relevantEvent in relevantEventsOfService)
                {
                    var eventSchemata = service.PublishedEventTypes.Single(ev => ev.Equals(relevantEvent));
                    var notFoundProperties = GetDiffOfProperties(relevantEvent, eventSchemata);

                    relevantEvents.Add(new EventSchema(relevantEvent.Name, notFoundProperties));
                }

                var relevantReadModels = new List<ReadModelSubscription>();
                var relevantReadModelsService = readModels.Where(r =>
                    service.PublishedEventTypes.Contains(r.GetsCreatedOn)).ToList();
                foreach (var readModel in relevantReadModelsService)
                {
                    var createdEvent = readModel.GetsCreatedOn;
                    var eventSchemata = service.PublishedEventTypes.Single(ev => ev.Equals(createdEvent));
                    var notFoundProperties = GetDiffOfProperties(createdEvent, eventSchemata);

                    relevantReadModels.Add(new ReadModelSubscription(
                        readModel.ReadModelName,
                        new EventSchema(createdEvent.Name, notFoundProperties)));
                }

                if (!relevantEvents.Any() && !relevantReadModels.Any()) continue;

                SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                    service.ServiceBaseAddress,
                    relevantEvents,
                    relevantReadModels,
                    service.ServiceName));
                UnresolvedEventSubscriptions = UnresolvedEventSubscriptions.Except(relevantEvents);
                UnresolvedReadModeSubscriptions = UnresolvedReadModeSubscriptions.Except(relevantReadModels);
            }
        }

        private static List<PropertyType> GetDiffOfProperties(EventSchema createdEvent, EventSchema eventSchemata)
        {
            var foundProperties = createdEvent.Properties.Where(p => eventSchemata.Properties.Contains(p));
            var notFoundProperties = createdEvent.Properties.Where(p => !eventSchemata.Properties.Contains(p)).ToList();

            var allProps = foundProperties.Select(p => new PropertyType(p.Name, p.Type, true)).ToList();
            notFoundProperties.AddRange(allProps);
            return notFoundProperties;
        }

        public EventLocation(
            IEnumerable<SubscriberEventAndReadmodelConfig> services, 
            IEnumerable<EventSchema> unresolvedEventSubscriptions,
            IEnumerable<ReadModelSubscription> unresolvedReadModeSubscriptions)
        {
            Services = services;
            UnresolvedEventSubscriptions = unresolvedEventSubscriptions;
            UnresolvedReadModeSubscriptions = unresolvedReadModeSubscriptions;
        }

        public IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; private set; }
            = new List<SubscriberEventAndReadmodelConfig>();
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }

        public SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType)
        {
            return Services.FirstOrDefault(s => s.SubscribedEvents.Any(e => e.Name == eventType.Name));
        }

        public SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type readModel)
        {
            return Services.FirstOrDefault(s => s.ReadModels.Any(rm => rm.ReadModelName == readModel.Name));
        }

        private void SetDomainEventLocation(SubscriberEventAndReadmodelConfig service)
        {
            var servicesWithoutAddedService = Services.Where(s => s.ServiceBaseAddress != service.ServiceBaseAddress);
            var services = servicesWithoutAddedService.Append(service);
            Services = services;
        }
    }
}