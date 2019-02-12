using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions.UnitTests.ExtendenDll
{
    public class TestDomainEventExtendedDll : IDomainEvent
    {
        public TestDomainEventExtendedDll(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestDomainEventExtendedDllHandler : IHandleAsync<TestDomainEventExtendedDll>
    {
        public Task HandleAsync(TestDomainEventExtendedDll domainEvent)
        {
            return Task.CompletedTask;
        }
    }
}