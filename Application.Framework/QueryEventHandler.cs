using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryEventHandler<T> where T : Query
    {
        private readonly IQeryRepository _qeryRepository;

        public QueryEventHandler(IQeryRepository qeryRepository)
        {
            _qeryRepository = qeryRepository;
        }

        public async Task Handle(DomainEvent domainEvent)
        {
            var query = await _qeryRepository.Load<T>();
            query.Handle(domainEvent);
            await _qeryRepository.Save(query);
        }
    }
}