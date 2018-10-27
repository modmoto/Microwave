using System.Linq;
using System.Threading.Tasks;
using Application.Framework;

namespace Adapters.Framework.EventStores
{
    public class TypeProjectionHandler : ITypeProjectionHandler
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVersionRepository _versionRepository;

        public TypeProjectionHandler(IEventRepository eventRepository, IVersionRepository versionRepository)
        {
            _eventRepository = eventRepository;
            _versionRepository = versionRepository;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);

                var version = await _versionRepository.GetVersionAsync("TypeProjectionHandler");
                var result = await _eventRepository.LoadEventsSince(version);
                foreach (var domainEvent in result.Value)
                {
                    await _eventRepository.AppendToTypeStream(domainEvent);
                }

                var lastVersion = result.Value.Any() ? result.Value.Last().Created : -1L;
                await _versionRepository.SaveVersion(new LastProcessedVersion("TypeProjectionHandler", lastVersion));
            }
        }
    }
}