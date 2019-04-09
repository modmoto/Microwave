using System;
using System.Linq;
using System.Net.Http;
using Microwave.Discovery;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient
    {

        public DomainEventClient(IEventLocation eventLocation)
        {
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First();
                var domainEventLocation = eventLocation.GetServiceForEvent(eventType);
                if (domainEventLocation == null) HasTheValidLocation = false;
                else BaseAddress = new Uri(domainEventLocation.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First();
                var consumingService = eventLocation.GetServiceForEvent(eventType);
                if (consumingService == null) HasTheValidLocation = false;
                else BaseAddress = new Uri(consumingService.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else
            {
                var readModelType = type.GetGenericArguments().First();
                var subscriberEventAndReadmodelConfig = eventLocation.GetServiceForReadModel(readModelType);
                if (subscriberEventAndReadmodelConfig == null) HasTheValidLocation = false;
                else BaseAddress = new Uri(subscriberEventAndReadmodelConfig.ServiceBaseAddress + "Api/DomainEvents");
            }
        }

        public bool HasTheValidLocation { get; } = true;

        public DomainEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}