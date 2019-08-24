using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.WebApi.Queries;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _asyncEventHandlers;
        private readonly IEnumerable<IQueryEventHandler> _queryHandlers;
        private readonly IEnumerable<IReadModelEventHandler> _readModelHandlers;
        private readonly IEnumerable<IPollingInterval> _updateEveryAttributes;
        private readonly IDiscoveryHandler _discoveryHandler;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            IDiscoveryHandler discoveryHandler,
            IEnumerable<IPollingInterval> updateEveryAttributes = null)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
            _updateEveryAttributes = updateEveryAttributes ?? new List<IPollingInterval>();
            _discoveryHandler = discoveryHandler;
        }

        public void StartEventPolling()
        {
            #pragma warning disable 4014
            foreach (var handler in _queryHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler));
            foreach (var handler in _readModelHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler));
            foreach (var handler in _asyncEventHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetUpdateEveryAttribute(handler.HandlerClassType));
            #pragma warning restore 4014
        }

        private IPollingInterval GetTimingAttribute(IQueryEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }

        private IPollingInterval GetTimingAttribute(IReadModelEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }

        private IPollingInterval GetUpdateEveryAttribute(Type type)
        {
            return _updateEveryAttributes.FirstOrDefault(u => u.AsyncCallType == type)
                   ?? new PollingInterval<Type>();
        }

        private void StartThreadForHandlingUpdates(Func<Task> action, IPollingInterval config)
        {
            Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var now = DateTime.UtcNow;
                            var nextTrigger = config.Next;
                            var timeSpan = nextTrigger - now;
                            await Task.Delay(timeSpan);
                            await action.Invoke();
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
                });
        }

        public async Task StartDependencyDiscovery()
        {
            while (true)
            {
                _discoveryHandler.DiscoverConsumingServices().Wait();
                await _discoveryHandler.SubscribeOnDiscoveredServices();
                await Task.Delay(60000);
            }
        }
    }
}