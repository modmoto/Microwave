using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
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
            var pollingInterval = new PollingInterval<AsyncEventHandler<Handler2, TestEv2>>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var scope = new Mock<IServiceScope>();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetService(typeof(AsyncEventHandler<Handler2, TestEv2>)))
                .Returns(new HandlerThatThrowException());
            scope.Setup(s => s.ServiceProvider).Returns(provider.Object);
            serviceScopeFactory.Setup(s => s.CreateScope()).Returns(scope.Object);
            var backgroundService =
                new MicrowaveBackgroundService<AsyncEventHandler<Handler2, TestEv2>>(serviceScopeFactory.Object,
                pollingInterval);

            backgroundService.StartAsync(CancellationToken.None);
            backgroundService.RunAsync();
        }
    }

    public class HandlerThatThrowException : IHandleAsync<TestEv2>
    {
        public Task HandleAsync(TestEv2 domainEvent)
        {
            throw new Exception();
        }
    }
}