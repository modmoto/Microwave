using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class NewDomainEventClient<T> : HttpClient
    {
        public NewDomainEventClient(IEventLocationConfig config)
        {
            BaseAddress = config.GetLocationForDomainEvent(typeof(T).Name);
        }

        public NewDomainEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}