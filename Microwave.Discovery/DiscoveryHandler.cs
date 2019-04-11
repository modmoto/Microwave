using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Discovery.Domain;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery
{
    public class DiscoveryHandler
    {
        private readonly ServiceBaseAddressCollection _serviceBaseAddressCollection;
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly IServiceDiscoveryRepository _discoveryRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly MicrowaveConfiguration _configuration;

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            EventsSubscribedByService eventsSubscribedByService,
            IServiceDiscoveryRepository discoveryRepository,
            IStatusRepository statusRepository,
            MicrowaveConfiguration configuration)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _eventsSubscribedByService = eventsSubscribedByService;
            _discoveryRepository = discoveryRepository;
            _statusRepository = statusRepository;
            _configuration = configuration;
        }

        public async Task<EventLocationDto> GetConsumingServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return new EventLocationDto(
                eventLocation.Services,
                eventLocation.UnresolvedEventSubscriptions,
                eventLocation.UnresolvedReadModeSubscriptions,
                "TestName");
        }

        public async Task<ServiceMap> GetServiceMap()
        {
            var serviceMap = await _statusRepository.GetServiceMap();
            return serviceMap;
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

        public async Task DiscoverServiceMap()
        {
            var allServices = new List<ServiceNodeWithDependentServicesDto>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetDependantServices(serviceAddress);
                allServices.Add(new ServiceNodeWithDependentServicesDto(
                    _configuration.ServiceName,
                    publishedEventTypes.Services.Select(s => s.ServiceEndPoint)));
            }

            var map = new ServiceMap(allServices);
            await _statusRepository.SaveServiceMap(map);
        }

        public async Task<ServiceNodeWithDependentServicesDto> GetConsumingServiceNodes()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return new ServiceNodeWithDependentServicesDto(
                _configuration.ServiceName,
                eventLocation.Services.Select(s => s.ServiceEndPoint));
        }
    }

    public class ServiceMap
    {
        public IEnumerable<ServiceNodeWithDependentServicesDto> AllServices { get; }

        public ServiceMap(IEnumerable<ServiceNodeWithDependentServicesDto> allServices)
        {
            AllServices = allServices;
        }
    }
}