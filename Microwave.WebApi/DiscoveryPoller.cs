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
        private Task _pollTask;

        public DiscoveryPoller(
            IDiscoveryHandler discoveryHandler,
            IMicrowaveLogger<DiscoveryPoller> logger)
        {
            _discoveryHandler = discoveryHandler;
            _logger = logger;
        }
        
        public void StartDependencyDiscovery()
        {
            _pollTask = new Task(() =>
            {
                while (true)
                {
                    Console.WriteLine($"_____ start wait:");

                    Task.Delay(10000).Wait();

                    Console.WriteLine($"_____ end wait:");
                    _discoveryHandler.DiscoverConsumingServices();

                    Task.Delay(50000);

                    Console.WriteLine($"_____ end poll wait:");
                }
                // ReSharper disable once FunctionNeverReturns
            });
            _pollTask.Start();
        }
    }
}