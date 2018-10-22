using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.WebApi
{
    public class PublishedEventStream<T> : IPublishedEventStream<T> where T : DomainEvent
    {
        private readonly IDomainEventConverter _domainEventConverter;
        private readonly DomainEventClient<T> _domainEventClient;

        public PublishedEventStream(IDomainEventConverter domainEventConverter, DomainEventClient<T>
            domainEventClient)
        {
            _domainEventConverter = domainEventConverter;
            _domainEventClient = domainEventClient;
        }

        public async Task<IEnumerable<T>> GetEventsByTypeAsync(long lastVersion)
        {
            var response = await _domainEventClient.GetAsync($"?myLastVersion={lastVersion}");
            if (response.StatusCode != HttpStatusCode.OK) return new List<T>();
            var content = await response.Content.ReadAsStringAsync();
            return _domainEventConverter.DeserializeList<T>(content);
        }
    }
}