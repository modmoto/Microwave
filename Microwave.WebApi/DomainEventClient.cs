using System.Linq;
using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient
    {
        public DomainEventClient(IEventLocationConfig config)
        {
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First();
                BaseAddress = config.GetLocationForDomainEvent(eventType.Name);
            }
            else if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First();
                BaseAddress = config.GetLocationForDomainEvent(eventType.Name);
            }
            else
            {
                var readModelType = type.GetGenericArguments().First();
                BaseAddress = config.GetLocationForReadModel(readModelType.Name);
            }
        }

        public DomainEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}