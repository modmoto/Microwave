using System.Net.Http;
using Microwave.Application.Ports;

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