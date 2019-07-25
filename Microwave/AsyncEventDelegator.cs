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
        private readonly IEnumerable<IUpdateEveryConfig> _updateEveryAttributes;
        private readonly IDiscoveryHandler _discoveryHandler;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            IDiscoveryHandler discoveryHandler,
            IEnumerable<IUpdateEveryConfig> updateEveryAttributes = null)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
            _updateEveryAttributes = updateEveryAttributes ?? new List<IUpdateEveryConfig>();
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

        private IUpdateEveryConfig GetTimingAttribute(IQueryEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }

        private IUpdateEveryConfig GetTimingAttribute(IReadModelEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return GetUpdateEveryAttribute(first);
        }

        private IUpdateEveryConfig GetUpdateEveryAttribute(Type type)
        {
            return _updateEveryAttributes.FirstOrDefault(u => u.AsyncCallType == type) ?? new
            DefaultConfig();
        }

        private void StartThreadForHandlingUpdates(Func<Task> action, IUpdateEveryConfig config)
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
                await _discoveryHandler.DiscoverConsumingServices();
                await Task.Delay(60000);
            }
        }
    }

    internal class DefaultConfig : IUpdateEveryConfig
    {
        public Type AsyncCallType { get; }
        public DateTime Next => DateTime.UtcNow;
    }
}