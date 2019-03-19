using System;
using System.Linq;
using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient
    {
        public DomainEventClient(ReadModelConfiguration config, EventLocation eventLocation)
        {
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First();
                var domainEventLocation = eventLocation.GetDomainEventLocation(eventType);
                BaseAddress = new Uri(domainEventLocation + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First();
                BaseAddress = new Uri(eventLocation.GetDomainEventLocation(eventType) + $"Api/DomainEventTypeStreams/{eventType.Name}");
            }
            else
            {
                var readModelType = type.GetGenericArguments().First();
                BaseAddress = new Uri(config.GetReadModelLocation(readModelType) + "Api/DomainEvents");
            }
        }

        public DomainEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}