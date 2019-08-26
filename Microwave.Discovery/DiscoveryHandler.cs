using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Discovery.Subscriptions;

namespace Microwave.Discovery
{
    public class DiscoveryHandler : IDiscoveryHandler
    {
        private readonly ServiceBaseAddressCollection _serviceBaseAddressCollection;
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly EventsPublishedByService _eventsPublishedByService;
        private readonly IServiceDiscoveryRepository _discoveryRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly DiscoveryConfiguration _configuration;

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            EventsSubscribedByService eventsSubscribedByService,
            EventsPublishedByService eventsPublishedByService,
            IServiceDiscoveryRepository discoveryRepository,
            IStatusRepository statusRepository,
            ISubscriptionRepository subscriptionRepository,
            DiscoveryConfiguration configuration)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _eventsSubscribedByService = eventsSubscribedByService;
            _eventsPublishedByService = eventsPublishedByService;
            _discoveryRepository = discoveryRepository;
            _statusRepository = statusRepository;
            _subscriptionRepository = subscriptionRepository;
            _configuration = configuration;
        }

        public async Task<EventLocation> GetConsumingServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return new EventLocation(
                eventLocation.Services,
                eventLocation.UnresolvedEventSubscriptions,
                eventLocation.UnresolvedReadModeSubscriptions);
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
            return MicrowaveServiceNode.ReachableMicrowaveServiceNode(new ServiceEndPoint(null, _configuration.ServiceName),
                eventLocation.Services.Select(s => s.ServiceEndPoint));
        }

        public Task<EventsPublishedByService> GetPublishedEvents()
        {
            return Task.FromResult(_eventsPublishedByService);
        }
    }
}