using Microwave.Queries;

namespace Microwave.UnitTests
{
    public class EventOnlySubbed : ISubscribedDomainEvent
    {
        public string S { get; }
        public string Addition { get; }

        public EventOnlySubbed(string s, string addition)
        {
            S = s;
            Addition = addition;
        }

        public string EntityId => S;
    }
}