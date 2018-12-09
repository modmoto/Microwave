using System.Net.Http;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient where T : IDomainEvent
    {
        public DomainEventClient(IEventLocationConfig config)
        {
            BaseAddress = config.GetLocationForDomainEvent(typeof(T).Name);
        }
    }
}