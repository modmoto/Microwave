using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;

namespace DependencyInjection.Framework.UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            var storeDependencies = collection.AddMyEventStoreDependencies(typeof(TestEventHandler).Assembly, config);
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var testEventHandler = buildServiceProvider.GetServices<IEventDelegateHandler>();
            var delegateHandler1 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent>>();
            var delegateHandler2 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent2>>();

            Assert.AreEqual(2, testEventHandler.Count());
            Assert.AreEqual(2, delegateHandler1.Count());
            Assert.AreEqual(1, delegateHandler2.Count());
        }
    }

    public class TestEventHandler : IHandleAsync<TestDomainEvent>, IHandleAsync<TestDomainEvent2>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(TestDomainEvent2 domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestEventHandler2 : IHandleAsync<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestDomainEvent2 : DomainEvent
    {
        public TestDomainEvent2(Guid entityId) : base(entityId)
        {
        }
    }

    public class TestDomainEvent : DomainEvent
    {
        public TestDomainEvent(Guid entityId) : base(entityId)
        {
        }
    }
}