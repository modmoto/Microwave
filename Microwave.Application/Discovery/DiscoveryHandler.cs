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

        public DiscoveryHandler(
            ServiceBaseAddressCollection serviceBaseAddressCollection,
            SubscribedEventCollection subscribedEventCollection,
            IServiceDiscoveryRepository discoveryRepository)
        {
            _serviceBaseAddressCollection = serviceBaseAddressCollection;
            _subscribedEventCollection = subscribedEventCollection;
            _discoveryRepository = discoveryRepository;
        }

        public async Task<ServiceConfig> GetConsumingServices()
        {
            var allServices = new List<ConsumingService>();
            foreach (var serviceAddress in _serviceBaseAddressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                allServices.Add(publishedEventTypes);
            }

            var relevantServices = new ServiceConfig();;

            var handleAsyncEvents = _subscribedEventCollection.IHandleAsyncEvents.ToList();

            foreach (var service in allServices)
            {
                var relevantEvents = service.PublishedEventTypes.Where(ev => handleAsyncEvents.Contains(ev)).ToList();
                if (relevantEvents.Any())
                {
                    relevantServices.Add(new ConsumingService(
                        service.ServiceBaseAddress,
                        relevantEvents,
                        service.ServiceName));
                }
            }

            return relevantServices;
        }
    }
}