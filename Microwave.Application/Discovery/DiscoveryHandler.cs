using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public class DiscoveryHandler
    {
        private readonly ServiceBaseAddressCollection _serviceBaseAddressCollection;
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly IServiceDiscoveryRepository _discoveryRepository;
        private readonly IStatusRepository _statusRepository;

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            EventsSubscribedByService eventsSubscribedByService,
            IServiceDiscoveryRepository discoveryRepository,
            IStatusRepository statusRepository)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _eventsSubscribedByService = eventsSubscribedByService;
            _discoveryRepository = discoveryRepository;
            _statusRepository = statusRepository;
        }

        public async Task<IEventLocation> GetConsumingServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return eventLocation;
        }

        public async Task DiscoverServiceMap()
        {
            var allServices = new List<ServiceDependenciesDto>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetDependantServices(serviceAddress);
                allServices.Add(publishedEventTypes);
            }

            var serviceMap = new ServiceMap(allServices);
            await _statusRepository.SaveServiceMap(serviceMap);
        }


        public async Task DiscoverConsumingServices()
        {
            var allServices = new List<EventsPublishedByService>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                allServices.Add(publishedEventTypes);
            }

            var eventLocation = new EventLocation(allServices, _eventsSubscribedByService);
            await _statusRepository.SaveEventLocation(eventLocation);
        }
    }

    public class ServiceMap
    {
        public IEnumerable<ServiceDependenciesDto> AllServices { get; }

        public ServiceMap(IEnumerable<ServiceDependenciesDto> allServices)
        {
            AllServices = allServices;
        }
    }
}