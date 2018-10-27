using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryEventHandler<T, TEvent> where T : Query where TEvent : DomainEvent
    {
        private readonly IQeryRepository _qeryRepository;

        public QueryEventHandler(IQeryRepository qeryRepository)
        {
            _qeryRepository = qeryRepository;
        }

        public async Task HandleAsync(TEvent domainEvent)
        {
            var result = await _qeryRepository.Load<T>();
            var query = result.Value;
            query.Handle(domainEvent);
            await _qeryRepository.Save(query);
        }
    }

    public class IdentifiableQueryEventHandler<T, TEvent> where T : IdentifiableQuery where TEvent : DomainEvent
    {
        private readonly IQeryRepository _qeryRepository;

        public IdentifiableQueryEventHandler(IQeryRepository qeryRepository)
        {
            _qeryRepository = qeryRepository;
        }

        public async Task HandleAsync(TEvent domainEvent)
        {
            var result = await _qeryRepository.Load<T>(domainEvent.EntityId);
            var identifiableQuery = result.Value;
            identifiableQuery.Handle(domainEvent);
            await _qeryRepository.Save(identifiableQuery);
        }
    }
}