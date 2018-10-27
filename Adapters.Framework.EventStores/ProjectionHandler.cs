using System.Linq;
using System.Threading.Tasks;
using Application.Framework;

namespace Adapters.Framework.EventStores
{
    public class ProjectionHandler : IProjectionHandler
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVersionRepository _versionRepository;

        public ProjectionHandler(IEventRepository eventRepository, IVersionRepository versionRepository)
        {
            _eventRepository = eventRepository;
            _versionRepository = versionRepository;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);
                var version = await _versionRepository.GetVersionAsync("AllDomainEventProjections");
                var result = await _eventRepository.LoadEventsSince(version);
                await _eventRepository.AppendToOverallStream(result.Value);
                var lastVersion = result.Value.Any() ? result.Value.Last().Created : -1L;
                await _versionRepository.SaveVersion(new LastProcessedVersion("AllDomainEventProjections", lastVersion));
            }
        }
    }
}