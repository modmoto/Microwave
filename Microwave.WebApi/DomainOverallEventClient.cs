using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainOverallEventClient<T> : HttpClient where T : ReadModel
    {
        public DomainOverallEventClient(IEventLocationConfig config)
        {
            BaseAddress = config.GetLocationForReadModel(typeof(T).Name);
        }

        public DomainOverallEventClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}