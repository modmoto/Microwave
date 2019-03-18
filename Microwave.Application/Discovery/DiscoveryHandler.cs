using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public class DiscoveryHandler
    {
        private readonly ServiceBaseAdressCollection _serviceBaseAdressCollection;
        private readonly SubscribedEventCollection _subscribedEventCollection;
        private readonly IServiceDiscoveryRepository _discoveryRepository;

        public DiscoveryHandler(
            ServiceBaseAdressCollection serviceBaseAdressCollection,
            SubscribedEventCollection subscribedEventCollection,
            IServiceDiscoveryRepository discoveryRepository)
        {
            _serviceBaseAdressCollection = serviceBaseAdressCollection;
            _subscribedEventCollection = subscribedEventCollection;
            _discoveryRepository = discoveryRepository;
        }

        public async Task<IEnumerable<ConsumingService>> GetConsumingServices()
        {
            var allServices = new List<ConsumingService>();
            foreach (var serviceAddress in _serviceBaseAdressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                allServices.Add(publishedEventTypes);
            }


            var relevantServices = new List<ConsumingService>();

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

    public interface IServiceDiscoveryRepository
    {
        Task<ConsumingService> GetPublishedEventTypes(Uri serviceAdress);
    }
}