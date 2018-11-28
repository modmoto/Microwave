using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;

namespace Adapters.Framework.EventStores
{
    public class TypeProjectionHandler : ITypeProjectionHandler
    {
        private readonly ITypeProjectionRepository _typeProjectionRepository;
        private readonly IEntityStreamRepository _overallProjectionRepository;
        private readonly IVersionRepository _versionRepository;

        public TypeProjectionHandler(
            ITypeProjectionRepository typeProjectionRepository,
            IEntityStreamRepository overallProjectionRepository,
            IVersionRepository versionRepository)
        {
            _typeProjectionRepository = typeProjectionRepository;
            _overallProjectionRepository = overallProjectionRepository;
            _versionRepository = versionRepository;
        }

        public async Task Update()
        {
            var version = await _versionRepository.GetVersionAsync("TypeProjectionHandler");
            // TOdo über api machen
            var result = await _overallProjectionRepository.LoadEventsSince(version);
            foreach (var domainEvent in result.Value)
            {
                await _typeProjectionRepository.AppendToTypeStream(domainEvent.DomainEvent);
            }

            var lastVersion = result.Value.Any() ? result.Value.Last().Created : version;
            await _versionRepository.SaveVersion(new LastProcessedVersion("TypeProjectionHandler", lastVersion));
        }
    }
}