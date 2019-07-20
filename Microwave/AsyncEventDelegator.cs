using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries;
using Microwave.WebApi.Querries;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _asyncEventHandlers;
        private readonly IEnumerable<IQueryEventHandler> _queryHandlers;
        private readonly IEnumerable<IReadModelEventHandler> _readModelHandlers;
        private readonly IDiscoveryHandler _discoveryHandler;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            IDiscoveryHandler discoveryHandler)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
            _discoveryHandler = discoveryHandler;
        }

        public void StartEventPolling()
        {
            #pragma warning disable 4014
            foreach (var handler in _queryHandlers) StartThreadForHandlingUpdates(() => handler.Update());
            foreach (var handler in _readModelHandlers) StartThreadForHandlingUpdates(() => handler.Update());
            foreach (var handler in _asyncEventHandlers) StartThreadForHandlingUpdates(() => handler.Update());
            #pragma warning restore 4014
        }

        private async Task StartThreadForHandlingUpdates(Func<Task> action)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(5000);
                    await action.Invoke();
                }
            }
            catch (DomainEventNotAssignableToEntityException notAssignableToEntityException)
            {
                var currentForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(notAssignableToEntityException.Message);
                Console.ForegroundColor = currentForeground;
            }
            catch (Exception e)
            {
                var currentForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Exception was thrown during a Async Handler, this queue is stuck now");
                Console.Error.WriteLine(e.ToString());
                Console.ForegroundColor = currentForeground;
            }
        }

        public async Task StartDependencyDiscovery()
        {
            while (true)
            {
                await _discoveryHandler.DiscoverConsumingServices();
                await Task.Delay(60000);
            }
        }
    }
}