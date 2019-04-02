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
        private IEventLocation _eventLocation;

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            SubscribedEventCollection subscribedEventCollection,
            IServiceDiscoveryRepository discoveryRepository,
            IEventLocation eventLocation)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _subscribedEventCollection = subscribedEventCollection;
            _discoveryRepository = discoveryRepository;
            _eventLocation = eventLocation;
        }

        public IEventLocation GetConsumingServices()
        {
            return _eventLocation;
        }

        public async Task DiscoverConsumingServices()
        {
            var allServices = new List<PublisherEventConfig>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                allServices.Add(publishedEventTypes);
            }

            _eventLocation = new EventLocation(allServices, _subscribedEventCollection);
        }
    }
}