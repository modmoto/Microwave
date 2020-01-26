using System;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Logging;

namespace Microwave.WebApi
{
    public class DiscoveryPoller
    {
        private readonly IDiscoveryHandler _discoveryHandler;
        private readonly IMicrowaveLogger<DiscoveryPoller> _logger;

        public DiscoveryPoller(
            IDiscoveryHandler discoveryHandler,
            IMicrowaveLogger<DiscoveryPoller> logger)
        {
            _discoveryHandler = discoveryHandler;
            _logger = logger;
        }
        
        public async Task StartDependencyDiscovery()
        {
            while (true)
            {
                Console.WriteLine("Microwave: Updating Service Discovery...");
                await _discoveryHandler.DiscoverConsumingServices();
                Console.WriteLine("Microwave: Finish Service Discovery.");
                await Task.Delay(60000);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}