using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microwave.Logging;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly List<IAsyncEventHandler> _asyncEventHandlers;
        private readonly List<IQueryEventHandler> _queryHandlers;
        private readonly List<IReadModelEventHandler> _readModelHandlers;
        private readonly IMicrowaveLogger<AsyncEventDelegator> _logger;
        private readonly IEnumerable<IPollingInterval> _updateEveryAttributes;
        public IList<ConfiguredTaskAwaitable> Tasks { get; private set; } = new List<ConfiguredTaskAwaitable>();

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            IMicrowaveLogger<AsyncEventDelegator> logger = null,
            IEnumerable<IPollingInterval> updateEveryAttributes = null)
        {
            _asyncEventHandlers = asyncEventHandlers.ToList();
            _queryHandlers = queryHandlers.ToList();
            _readModelHandlers = readModelHandlers.ToList();
            _logger = logger ?? new MicrowaveLogger<AsyncEventDelegator>();
            _updateEveryAttributes = updateEveryAttributes ?? new List<IPollingInterval>();
        }

        public void Reset()
        {
            Tasks = new List<ConfiguredTaskAwaitable>();
        }

        public void StartEventPolling()
        {
            #pragma warning disable 4014
            Console.WriteLine($"Start threads for {_queryHandlers.Count} QuerryHandlers");
            foreach (var handler in _queryHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler),
                GetInterestingName(handler));

            Console.WriteLine($"Start threads for {_queryHandlers.Count} ReadModelHandlers");
            foreach (var handler in _readModelHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler),
                GetInterestingName(handler));

            Console.WriteLine($"Start threads for {_queryHandlers.Count} AsyncEventHandlers");
            foreach (var handler in _asyncEventHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetUpdateEveryAttribute(handler.HandlerClassType),
                handler.HandlerClassType);
            #pragma warning restore 4014
        }

        private IPollingInterval GetTimingAttribute(IQueryEventHandler handler)
        {
            var first = GetInterestingName(handler);
            return GetUpdateEveryAttribute(first);
        }

        private IPollingInterval GetTimingAttribute(IReadModelEventHandler handler)
        {
            var first = GetInterestingName(handler);
            return GetUpdateEveryAttribute(first);
        }

        private static Type GetInterestingName(IReadModelEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return first;
        }

        private static Type GetInterestingName(IQueryEventHandler handler)
        {
            var type = handler.GetType();
            var first = type.GenericTypeArguments.First();
            return first;
        }

        private IPollingInterval GetUpdateEveryAttribute(Type type)
        {
            return _updateEveryAttributes.FirstOrDefault(u => u.AsyncCallType == type)
                   ?? new PollingInterval<Type>();
        }

        private void StartThreadForHandlingUpdates(Func<Task> action, IPollingInterval config, Type loggingType)
        {
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var now = DateTime.UtcNow;
                        var nextTrigger = config.Next;
                        var timeSpan = nextTrigger - now;
                        await Task.Delay(timeSpan);
                        Console.WriteLine($"Start handling events for {loggingType.Name}");
                        await action.Invoke();
                        Console.WriteLine($"sucessfully handled events for {loggingType.Name}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e,
                            "Microwave: Exception was thrown during a Async Handler, this queue is stuck now");
                    }
                }

                // ReSharper disable once FunctionNeverReturns
            }).ConfigureAwait(false);
            Tasks.Add(task);
        }
    }
}