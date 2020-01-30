using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microwave.Logging;
using Microwave.Queries.Handler;

namespace Microwave.Queries.Polling
{

    public class MicrowaveBackgroundService<T> : IHostedService where T : IEventHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PollingInterval<T> _pollingInterval;

        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public MicrowaveBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _pollingInterval = scope.ServiceProvider.GetService<PollingInterval<T>>();
            }
        }

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
                try
                {
                    var now = DateTime.UtcNow;
                    var nextTrigger = _pollingInterval.Next;
                    var timeSpan = nextTrigger - now;
                    await Task.Delay(timeSpan);
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetService<T>();
                        await service.Update();
                    }
                }
                catch (Exception e)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetService<IMicrowaveLogger<MicrowaveBackgroundService<T>>>();
                        logger.LogWarning(e, $"Microwave: Error occured in async handler of {typeof(T).GetGenericArguments().First().Name}");
                    }
                }
            }
            while (!stoppingToken.IsCancellationRequested);
        }
    }
}