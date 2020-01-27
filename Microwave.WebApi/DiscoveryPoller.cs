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
        public ConfiguredTaskAwaitable<Task> PollTask { get; private set; }

        public DiscoveryPoller(
            IDiscoveryHandler discoveryHandler,
            IMicrowaveLogger<DiscoveryPoller> logger)
        {
            _discoveryHandler = discoveryHandler;
            _logger = logger;
        }
        
        public void StartDependencyDiscovery()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"_____ start wait:");

                Task.Delay(10000).Wait();

                Console.WriteLine($"_____ end wait:");
                _discoveryHandler.DiscoverConsumingServices().Wait();
                var microwaveServiceNodes = _discoveryHandler.GetConsumingServices().Result.Services;

                foreach (var serviceNode in microwaveServiceNodes)
                {
                    Console.WriteLine(serviceNode.ServiceEndPoint);
                }
                // Task.Delay(50000).Wait();

                Console.WriteLine($"_____ end poll wait:");
            }
        }
    }
}