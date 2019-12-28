using System.Threading.Tasks;
using Microwave.Discovery;

namespace Microwave.WebApi
{
    public class DiscoveryPoller
    {
        private readonly IDiscoveryHandler _discoveryHandler;

        public DiscoveryPoller(IDiscoveryHandler discoveryHandler)
        {
            _discoveryHandler = discoveryHandler;
        }
        
        public async Task StartDependencyDiscovery()
        {
            while (true)
            {
                await _discoveryHandler.DiscoverConsumingServices();
                await Task.Delay(60000);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}