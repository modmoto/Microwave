using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Domain;

namespace Microwave.Discovery
{
    public class DiscoveryHandler : IDiscoveryHandler
    {
        private readonly IServiceBaseAddressCollection _serviceBaseAddressCollection;
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly IServiceDiscoveryRepository _discoveryRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IMicrowaveConfiguration _configuration;

        public DiscoveryHandler(
            IServiceBaseAddressCollection serviceBaseAddressCollection,
            EventsSubscribedByService eventsSubscribedByService,
            IServiceDiscoveryRepository discoveryRepository,
            IStatusRepository statusRepository,
            IMicrowaveConfiguration configuration)
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
            if (eventLocation != null)
            {
                return new EventLocationDto(
                    eventLocation.Services,
                    eventLocation.UnresolvedEventSubscriptions,
                    eventLocation.UnresolvedReadModeSubscriptions,
                    "TestName");
            }

            return null;
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
            var allServices = new List<MicrowaveServiceNode>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var node = await _discoveryRepository.GetDependantServices(serviceAddress);
                allServices.Add(node);
            }

            var map = new ServiceMap(allServices);
            await _statusRepository.SaveServiceMap(map);
        }

        public async Task<MicrowaveServiceNode> GetConsumingServiceNodes()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return new MicrowaveServiceNode(
                new ServiceEndPoint(null, _configuration.ServiceName),
                eventLocation.Services.Select(s => s.ServiceEndPoint),
                true);
        }
    }
}