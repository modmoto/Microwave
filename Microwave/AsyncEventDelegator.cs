using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries;
using Microwave.Queries.Handler;
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

        private UpdateEveryAttribute GetTimingAttribute(IQueryEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }

        private UpdateEveryAttribute GetTimingAttribute(IReadModelEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }
        private static UpdateEveryAttribute GetUpdateEveryAttribute(Type type)
        {
            var customAttribute = type.GetCustomAttribute(typeof(UpdateEveryAttribute));
            var updateEveryAttribute = customAttribute as UpdateEveryAttribute;
            return updateEveryAttribute ?? UpdateEveryAttribute.Default();
        }

        private async Task StartThreadForHandlingUpdates(Func<Task> action, UpdateEveryAttribute attribute)
        {
            try
            {
                while (true)
                {
                    var now = DateTime.UtcNow;
                    var nextTrigger = attribute.Next;
                    var timeSpan = nextTrigger - now;
                    await Task.Delay(timeSpan);
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