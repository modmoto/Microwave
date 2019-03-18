using System;
using System.Collections.Generic;
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
            var serviceConfigsTemp = new List<ConsumingService>();
            foreach (var serviceAddress in _serviceBaseAdressCollection)
            {
                var publishedEventTypes = await _discoveryRepository.GetPublishedEventTypes(serviceAddress);
                serviceConfigsTemp.Add(publishedEventTypes);
            }


            return serviceConfigsTemp;
        }
    }

    public interface IServiceDiscoveryRepository
    {
        Task<ConsumingService> GetPublishedEventTypes(Uri serviceAdress);
    }
}