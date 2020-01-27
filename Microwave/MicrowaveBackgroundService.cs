using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microwave.Queries.Handler;

namespace Microwave
{

    public abstract class BackgroundService : IHostedService
    {

        // protected readonly IPollingInterval UpdateEvery;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        // public BackgroundService(IEnumerable<IPollingInterval> updateEveryAttributes)
        // {
        //     UpdateEvery = updateEveryAttributes.FirstOrDefault(u => u.AsyncCallType == HandlerType) ?? new PollingInterval<Type>();
        // }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                    cancellationToken));
            }
        }

        protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                // var now = DateTime.UtcNow;
                // var nextTrigger = UpdateEvery.Next;
                // var timeSpan = nextTrigger - now;
                // await Task.Delay(timeSpan);
                await Task.Delay(1000);
                await Process();
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        protected abstract Task Process();
        protected abstract Type HandlerType { get; }
    }

    public class ReadModelBackgroundService : BackgroundService
    {
        private readonly IReadModelEventHandler _handler;

        public ReadModelBackgroundService(
            IReadModelEventHandler handler)
        {
            _handler = handler;
        }

        protected override async Task Process()
        {
            await _handler.Update();
        }

        protected override Type HandlerType => _handler.GetType();
    }

    public class QueryBackgroundService : BackgroundService
    {
        private readonly IQueryEventHandler _handler;

        public QueryBackgroundService(IQueryEventHandler handler)
        {
            _handler = handler;
        }

        protected override async Task Process()
        {
            await _handler.Update();
        }

        protected override Type HandlerType => _handler.GetType();
    }

    public class AsyncEventBackgroundService : BackgroundService
    {
        private readonly IAsyncEventHandler _handler;

        public AsyncEventBackgroundService(IAsyncEventHandler handler)
        {
            _handler = handler;
        }

        protected override async Task Process()
        {
            await _handler.Update();
        }

        protected override Type HandlerType => _handler.GetType();
    }
}