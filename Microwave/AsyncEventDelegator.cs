using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Logging;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _asyncEventHandlers;
        private readonly IEnumerable<IQueryEventHandler> _queryHandlers;
        private readonly IEnumerable<IReadModelEventHandler> _readModelHandlers;
        private readonly IMicrowaveLogger<AsyncEventDelegator> _logger;
        private readonly IEnumerable<IPollingInterval> _updateEveryAttributes;
        private readonly IEnumerable<Task> _tasks = new List<Task>();

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            IMicrowaveLogger<AsyncEventDelegator> logger = null,
            IEnumerable<IPollingInterval> updateEveryAttributes = null)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
            _logger = logger ?? new MicrowaveLogger<AsyncEventDelegator>();
            _updateEveryAttributes = updateEveryAttributes ?? new List<IPollingInterval>();
        }

        public void StartEventPolling()
        {
            #pragma warning disable 4014
            Console.WriteLine($"Start threads for {_queryHandlers.Count()} QuerryHandlers");
            foreach (var handler in _queryHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler),
                GetInterestingName(handler));

            Console.WriteLine($"Start threads for {_queryHandlers.Count()} ReadModelHandlers");
            foreach (var handler in _readModelHandlers) StartThreadForHandlingUpdates(
                () => handler.Update(),
                GetTimingAttribute(handler),
                GetInterestingName(handler));

            Console.WriteLine($"Start threads for {_queryHandlers.Count()} AsyncEventHandlers");
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
            var task = new Task(async () =>
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
            });
            task.Start();
            _tasks.Append(task);
        }
    }
}