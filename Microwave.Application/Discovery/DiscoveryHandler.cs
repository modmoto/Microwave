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
        private readonly IEventLocation _eventLocation;

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

            var handleAsyncEvents = _subscribedEventCollection.IHandleAsyncEvents.ToList();
            var readModels = _subscribedEventCollection.ReadModelSubcriptions.ToList();

            foreach (var service in allServices)
            {
                var relevantEvents = service.PublishedEventTypes.Where(ev => handleAsyncEvents.Contains(ev)).ToList();
                var relevantReadModels = readModels.Where(r =>
                        service.PublishedEventTypes.Contains(r.GetsCreatedOn)).ToList();

                if (relevantEvents.Any() || relevantReadModels.Any())
                {
                    _eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                        service.ServiceBaseAddress,
                        relevantEvents,
                        relevantReadModels,
                        service.ServiceName));
                }
            }

        }
    }
}