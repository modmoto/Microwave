using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microwave.Discovery;

namespace Microwave.WebApi
{
    public class DiscoveryPoller
    {
        private readonly IDiscoveryHandler _discoveryHandler;
        private readonly ILogger<DiscoveryPoller> _logger;

        public DiscoveryPoller(IDiscoveryHandler discoveryHandler, ILogger<DiscoveryPoller> logger)
        {
            _discoveryHandler = discoveryHandler;
            _logger = logger;
        }
        
        public async Task StartDependencyDiscovery()
        {
            while (true)
            {
                _logger.LogInformation("Updating Service Discovery...");
                await _discoveryHandler.DiscoverConsumingServices();
                await Task.Delay(60000);
                _logger.LogInformation("Finish Service Discovery.");
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}