using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Ports;

namespace Microwave.EventStores
{
    public class TypeProjectionHandler : ITypeProjectionHandler
    {
        private readonly ITypeProjectionRepository _typeProjectionRepository;
        private readonly IEntityStreamRepository _entityStreamRepository;
        private readonly IVersionRepository _versionRepository;

        public TypeProjectionHandler(
            ITypeProjectionRepository typeProjectionRepository,
            IEntityStreamRepository entityStreamRepository,
            IVersionRepository versionRepository)
        {
            _typeProjectionRepository = typeProjectionRepository;
            _entityStreamRepository = entityStreamRepository;
            _versionRepository = versionRepository;
        }

        public async Task Update()
        {
            var version = await _versionRepository.GetVersionAsync("TypeProjectionHandler");
            var result = await _entityStreamRepository.LoadEventsSince(version);
            foreach (var domainEvent in result.Value)
            {
                await _typeProjectionRepository.AppendToTypeStream(domainEvent.DomainEvent);
            }

            var lastVersion = result.Value.Any() ? result.Value.Last().Created : version;
            await _versionRepository.SaveVersion(new LastProcessedVersion("TypeProjectionHandler", lastVersion));
        }
    }
}