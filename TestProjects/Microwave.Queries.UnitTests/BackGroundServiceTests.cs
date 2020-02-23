using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.Queries.Ports;
using Moq;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class BackGroundServiceTests
    {
        [TestMethod]
        public void NextRunIsSet()
        {
            var pollingInterval = new PollingInterval<AsyncEventHandler<Handler2, TestEv2>>();
            var backgroundService =
                new MicrowaveBackgroundService<AsyncEventHandler<Handler2, TestEv2>>(null, pollingInterval);

            Assert.IsNotNull(backgroundService.NextRun);
        }

        [TestMethod]
        public void HandlerThrowsExceptionAndDoesNotDie()
        {
            var backgroundService = CreateBackGroundServiceFo<HandlerThatThrowException, TestEv2>();

            backgroundService.StartAsync(CancellationToken.None);
            backgroundService.RunAsync();
            backgroundService.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public void HandlerWorksFine()
        {
            var backgroundService = CreateBackGroundServiceFo<Handler2, TestEv2>();

            backgroundService.StartAsync(CancellationToken.None);
            backgroundService.RunAsync();
            backgroundService.StopAsync(CancellationToken.None);
        }

        private MicrowaveBackgroundService<AsyncEventHandler<THandler, TEvent>> CreateBackGroundServiceFo<THandler, TEvent>()
            where TEvent : ISubscribedDomainEvent where THandler : new()
        {
            var pollingInterval = new PollingInterval<AsyncEventHandler<THandler, TEvent>>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var scope = new Mock<IServiceScope>();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetService(typeof(AsyncEventHandler<THandler, TEvent>)))
                .Returns(new HandlerMock());
            scope.Setup(s => s.ServiceProvider).Returns(provider.Object);
            serviceScopeFactory.Setup(s => s.CreateScope()).Returns(scope.Object);
            var backgroundService =
                new MicrowaveBackgroundService<AsyncEventHandler<THandler, TEvent>>(serviceScopeFactory.Object,
                    pollingInterval);
            return backgroundService;
        }
    }

    internal class HandlerMock : IEventHandler
    {
        public Task UpdateAsync()
        {
            return Task.CompletedTask;
        }
    }

    public class HandlerThatThrowException : IEventHandler
    {
        public Task UpdateAsync()
        {
            throw new Exception();
        }
    }
}