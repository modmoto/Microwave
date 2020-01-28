using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries.Handler;

namespace Microwave.WebApi
{
    public class DiscoveryPoller : IEventHandler
    {
        private readonly IDiscoveryHandler _discoveryHandler;

        public DiscoveryPoller(
            IDiscoveryHandler discoveryHandler)
        {
            _discoveryHandler = discoveryHandler;
        }

        public async Task Update()
        {
            await _discoveryHandler.DiscoverConsumingServices();
        }
    }
}