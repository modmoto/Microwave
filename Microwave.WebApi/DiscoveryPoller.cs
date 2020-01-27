using System;
using System.Runtime.CompilerServices;
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
        
        public void StartDependencyDiscovery()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    Console.WriteLine($"_____ start wait:");

                    await Task.Delay(10000);

                    Console.WriteLine($"_____ end wait:");
                    await _discoveryHandler.DiscoverConsumingServices();

                    await Task.Delay(50000);

                    Console.WriteLine($"_____ end poll wait:");
                }
                // ReSharper disable once FunctionNeverReturns
            }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }
    }
}