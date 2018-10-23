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
            // TODO Threadsafe or occ
            var query = _qeryRepository.Load<T>().Result;
            query.Handle(domainEvent);
            _qeryRepository.Save(query).Wait();
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
            var query = _qeryRepository.Load<T>(domainEvent.EntityId).Result;
            query.Handle(domainEvent);
            _qeryRepository.Save(query).Wait();
        }
    }
}