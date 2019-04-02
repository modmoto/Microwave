using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application;
using Microwave.Application.Discovery;

namespace Microwave.WebApi
{
    public class EventLocation : IEventLocation
    {
        public IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; private set; }
            = new List<SubscriberEventAndReadmodelConfig>();

        public void Reset(List<PublisherEventConfig> allServices, SubscribedEventCollection subscribedEventCollection)
        {
            var readModels = subscribedEventCollection.ReadModelSubcriptions.ToList();
            var handleAsyncEvents = subscribedEventCollection.IHandleAsyncEvents.ToList();

            Services = new List<SubscriberEventAndReadmodelConfig>();

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