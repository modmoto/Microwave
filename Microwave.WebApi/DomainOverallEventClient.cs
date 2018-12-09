using System.Net.Http;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class DomainOverallEventClient<T> : HttpClient where T : ReadModel
    {
        public DomainOverallEventClient(IEventLocationConfig config)
        {
            BaseAddress = config.GetLocationFor(typeof(T).Name);
        }
    }
}