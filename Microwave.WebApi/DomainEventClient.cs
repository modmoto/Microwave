using System;
using System.Linq;
using System.Net.Http;
using Microwave.Application.Discovery;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient
    {
        private readonly IEventLocation _eventLocation;

        public DomainEventClient(IEventLocation eventLocation)
        {
            _eventLocation = eventLocation;
        }

        public bool Init()
        {
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First();
                var domainEventLocation = _eventLocation.GetServiceForEvent(eventType);
                if (domainEventLocation == null) return false;
                BaseAddress = new Uri(domainEventLocation.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First();
                var consumingService = _eventLocation.GetServiceForEvent(eventType);
                if (consumingService == null) return false;
                BaseAddress = new Uri(consumingService.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else
            {
                var readModelType = type.GetGenericArguments().First();
                var subscriberEventAndReadmodelConfig = _eventLocation.GetServiceForReadModel(readModelType);
                if (subscriberEventAndReadmodelConfig == null) return false;
                BaseAddress = new Uri(subscriberEventAndReadmodelConfig.ServiceBaseAddress + "Api/DomainEvents");
            }

            return true;
        }

        public DomainEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}