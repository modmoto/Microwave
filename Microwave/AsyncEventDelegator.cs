using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _asyncEventHandlers;
        private readonly IEnumerable<IQueryEventHandler> _queryHandlers;
        private readonly IEnumerable<IReadModelEventHandler> _readModelHandlers;
        private readonly ILogger<AsyncEventDelegator> _logger;
        private readonly IEnumerable<IPollingInterval> _updateEveryAttributes;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelEventHandler> readModelHandlers,
            ILogger<AsyncEventDelegator> logger = null,
            IEnumerable<IPollingInterval> updateEveryAttributes = null)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
            _logger = logger;
            _updateEveryAttributes = updateEveryAttributes ?? new List<IPollingInterval>();
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
                        catch (Exception e)
                        {
                            _logger?.LogError(e, "Microwave: Exception was thrown during a Async Handler, this queue is stuck now");
                        }
                    }
                    // ReSharper disable once FunctionNeverReturns
                });
        }
    }
}