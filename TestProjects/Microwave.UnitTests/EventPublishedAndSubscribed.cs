using Microwave.Domain.EventSourcing;
using Microwave.Queries;

namespace Microwave.UnitTestsPublished
{
    public class EventPublishedAndSubscribed : IDomainEvent
    {
        public string S { get; }
        public string Addition { get; }

        public EventPublishedAndSubscribed(string s, string addition)
        {
            S = s;
            Addition = addition;
        }

        public string EntityId => S;
    }
}

namespace Microwave.UnitTestsSubscribed
{
    public class EventPublishedAndSubscribed : ISubscribedDomainEvent
    {
        public string S { get; }
        public string Addition { get; }

        public EventPublishedAndSubscribed(string s, string addition)
        {
            S = s;
            Addition = addition;
        }

        public string EntityId => S;
    }
}