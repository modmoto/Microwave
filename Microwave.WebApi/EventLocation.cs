using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application.Discovery;

namespace Microwave.WebApi
{
    public class EventLocation : IEventLocation
    {
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