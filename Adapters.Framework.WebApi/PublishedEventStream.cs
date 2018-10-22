using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.WebApi
{
    public class PublishedEventStream : IPublishedEventStream
    {
        private readonly IEventLocationConfig _config;
        private readonly IDomainEventConverter _domainEventConverter;

        public PublishedEventStream(IEventLocationConfig config, IDomainEventConverter domainEventConverter)
        {
            _config = config;
            _domainEventConverter = domainEventConverter;
        }

        public async Task<IEnumerable<T>> GetEventsByTypeAsync<T>(long lastVersion) where T : DomainEvent
        {
            using (var client = new HttpClient())
            {
                var clientBaseAddress = _config.GetLocationFor(typeof(T).Name);
                client.BaseAddress = clientBaseAddress;
                var response = await client.GetAsync($"?myLastVersion={lastVersion}");
                if (response.StatusCode != HttpStatusCode.OK) return new List<T>();
                var content = await response.Content.ReadAsStringAsync();
                return _domainEventConverter.DeserializeList<T>(content);
            }
        }
    }
}