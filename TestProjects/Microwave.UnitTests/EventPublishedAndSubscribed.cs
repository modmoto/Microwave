using Microwave.Domain.EventSourcing;
using Microwave.Queries;

namespace Microwave.UnitTestsPublished
{
    public class EventPublishedAndSubscribed : IDomainEvent
    {
        public string S { get; }

        public EventPublishedAndSubscribed(string s)
        {
            S = s;
        }

        public string EntityId => S;
    }
}

namespace Microwave.UnitTestsSubscribed
{
    public class EventPublishedAndSubscribed : ISubscribedDomainEvent
    {
        public string S { get; }

        public EventPublishedAndSubscribed(string s)
        {
            S = s;
        }

        public string EntityId => S;
    }
}