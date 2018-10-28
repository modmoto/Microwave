using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class OverallProjectionRepository : IOverallProjectionRepository
    {
        private readonly ITypeProjectionRepository _typeProjectionRepository;

        public OverallProjectionRepository(ITypeProjectionRepository typeProjectionRepository)
        {
            _typeProjectionRepository = typeProjectionRepository;
        }

        public async Task<Result> AppendToOverallStream(IEnumerable<DomainEvent> events)
        {
            foreach (var domainEvent in events)
            {
                await _typeProjectionRepository.AppendToStreamWithName("AllDomainEvents", domainEvent);
            }
            return Result.Ok();
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadOverallStream(long from = -1)
        {
            return await _typeProjectionRepository.LoadEventsByTypeAsync("AllDomainEvents", from);
        }
    }
}