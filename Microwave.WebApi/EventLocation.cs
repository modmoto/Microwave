using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application.Discovery;

namespace Microwave.WebApi
{
    public class EventLocation : IEventLocation
    {
        private IEnumerable<SubscriberEventAndReadmodelConfig> _services = new List<SubscriberEventAndReadmodelConfig>();

        public SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType)
        {
            return _services.FirstOrDefault(s => s.SubscribedEvents.Contains(eventType.Name));
        }

        public SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type readModel)
        {
            return _services.FirstOrDefault(s => s.ReadModels.Any(rm => rm.ReadModelName == readModel.Name));
        }

        public void SetDomainEventLocation(SubscriberEventAndReadmodelConfig service)
        {
            _services = _services.Append(service);
        }
    }
}