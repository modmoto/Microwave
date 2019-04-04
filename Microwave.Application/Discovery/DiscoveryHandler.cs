using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public class DiscoveryHandler
    {
        private readonly ServiceBaseAddressCollection _serviceBaseAddressCollection;
        private readonly SubscribedEventCollection _subscribedEventCollection;
        private readonly IServiceDiscoveryRepository _discoveryRepository;
        private readonly IStatusRepository _statusRepository;

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            SubscribedEventCollection subscribedEventCollection,
            IServiceDiscoveryRepository discoveryRepository,
            IStatusRepository statusRepository)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _subscribedEventCollection = subscribedEventCollection;
            _discoveryRepository = discoveryRepository;
            _statusRepository = statusRepository;
        }

        public async Task<IEventLocation> GetConsumingServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            return eventLocation;
        }

        public async Task DiscoverConsumingServices()
        {
            var allServices = new List<PublisherEventConfig>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                allServices.Add(publishedEventTypes);
            }

            var eventLocation = new EventLocation(allServices, _subscribedEventCollection);
            await _statusRepository.SaveEventLocation(eventLocation);
        }
    }
}