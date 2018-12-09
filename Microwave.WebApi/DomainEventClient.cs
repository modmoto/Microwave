using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainEventClient<T> : HttpClient
    {
        public DomainEventClient(IEventLocationConfig config)
        {
            BaseAddress = config.GetLocationFor(typeof(T).Name);
        }
    }
}