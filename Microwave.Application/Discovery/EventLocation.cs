using System;
using System.Collections.Generic;
using System.Linq;

namespace Microwave.Application.Discovery
{
    public class EventLocation : IEventLocation
    {
        public EventLocation()
        {
        }

        public EventLocation(List<PublisherEventConfig> allServices, SubscribedEventCollection subscribedEventCollection)
        {
            var readModels = subscribedEventCollection.ReadModelSubcriptions.ToList();
            var handleAsyncEvents = subscribedEventCollection.IHandleAsyncEvents.ToList();

            foreach (var service in allServices)
            {
                var relevantEvents = service.PublishedEventTypes.Where(ev => handleAsyncEvents.Contains(ev)).ToList();
                var relevantReadModels = readModels.Where(r =>
                    service.PublishedEventTypes.Contains(r.GetsCreatedOn)).ToList();

                if (relevantEvents.Any() || relevantReadModels.Any())
                {
                    SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                        service.ServiceBaseAddress,
                        relevantEvents,
                        relevantReadModels,
                        service.ServiceName));
                }
            }
        }
        public IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; private set; }
            = new List<SubscriberEventAndReadmodelConfig>();

        public SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType)
        {
            return Services.FirstOrDefault(s => s.SubscribedEvents.Contains(eventType.Name));
        }

        public SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type readModel)
        {
            return Services.FirstOrDefault(s => s.ReadModels.Any(rm => rm.ReadModelName == readModel.Name));
        }

        public void SetDomainEventLocation(SubscriberEventAndReadmodelConfig service)
        {
            var servicesWithoutAddedService = Services.Where(s => s.ServiceBaseAddress != service.ServiceBaseAddress);
            var services = servicesWithoutAddedService.Append(service);
            Services = services;
        }
    }
}