using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class OverallProjectionRepository : IOverallProjectionRepository
    {
        private readonly ITypeProjectionRepository _typeProjectionRepository;

        public OverallProjectionRepository(ITypeProjectionRepository typeProjectionRepository)
        {
            _typeProjectionRepository = typeProjectionRepository;
        }

        public async Task<Result> AppendToOverallStream(IEnumerable<IDomainEvent> events)
        {
            foreach (var domainEvent in events)
            {
                await _typeProjectionRepository.AppendToStreamWithName("AllDomainEvents", domainEvent);
            }
            return Result.Ok();
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadOverallStream(long from = 0)
        {
            return await _typeProjectionRepository.LoadEventsByTypeAsync("AllDomainEvents", from);
        }
    }
}