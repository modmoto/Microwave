using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.ObjectPersistences;
using Microwave.Queries;

namespace Microwave.WebApi
{
    public class ReadModelFeed<T> : IOverallEventFeed where T : ReadModel
    {
        private readonly DomainEventWrapperListDeserializer _objectConverter;
        private readonly DomainOverallEventClient<T> _domainEventClient;

        public ReadModelFeed(
            DomainEventWrapperListDeserializer objectConverter,
            DomainOverallEventClient<T> domainEventClient)
        {
            _objectConverter = objectConverter;
            _domainEventClient = domainEventClient;
        }

        public async Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            var response = await _domainEventClient.GetAsync($"?timeStamp={lastVersion}");
            if (response.StatusCode != HttpStatusCode.OK) return new List<DomainEventWrapper>();
            var content = await response.Content.ReadAsStringAsync();
            var eventsByTypeAsync = _objectConverter.Deserialize(content);
            return eventsByTypeAsync;
        }
    }
}