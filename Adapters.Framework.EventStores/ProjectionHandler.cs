using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;

namespace Microwave.EventStores
{
    public class ProjectionHandler : IProjectionHandler
    {
        private readonly IOverallProjectionRepository _overallProjectionRepository;
        private readonly IEntityStreamRepository _streamRepository;
        private readonly IVersionRepository _versionRepository;

        public ProjectionHandler(
            IOverallProjectionRepository overallProjectionRepository,
            IEntityStreamRepository streamRepository,
            IVersionRepository versionRepository)
        {
            _overallProjectionRepository = overallProjectionRepository;
            _streamRepository = streamRepository;
            _versionRepository = versionRepository;
        }

        public async Task Update()
        {
            var version = await _versionRepository.GetVersionAsync("AllDomainEventProjections");
            //TODO [ber api machen
            var result = await _streamRepository.LoadEventsSince(version);
            var domainEventWrappers = result.Value.Select(ev => ev.DomainEvent);
            await _overallProjectionRepository.AppendToOverallStream(domainEventWrappers);
            var lastVersion = result.Value.Any() ? result.Value.Last().Created : version;
            await _versionRepository.SaveVersion(new LastProcessedVersion("AllDomainEventProjections", lastVersion));
        }
    }
}