using System.Threading.Tasks;
using Microwave.Queries;

namespace Microwave.UnitTests.PublishedEventsDll
{
    public class TestHandle : IHandleAsync<TestDomainEvent_PublishedEvent1>
    {
        public Task HandleAsync(TestDomainEvent_PublishedEvent1 domainEvent)
        {
            return null;
        }
    }

    public class TestHandle_Duplicate : IHandleAsync<TestDomainEvent_PublishedEvent1>
    {
        public Task HandleAsync(TestDomainEvent_PublishedEvent1 domainEvent)
        {
            return null;
        }
    }

    public class TestHandle2: IHandleAsync<TestDomainEvent_PublishedEvent2>
    {
        public Task HandleAsync(TestDomainEvent_PublishedEvent2 domainEvent)
        {
            return null;
        }
    }
}