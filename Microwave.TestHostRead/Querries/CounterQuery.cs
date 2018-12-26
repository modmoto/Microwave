using Microwave.Queries;
using Microwave.TestHostRead.DomainEvents;

namespace Microwave.TestHostRead.Querries
{
    public class CounterQuery : Query, IHandle<TeamCreated>
    {
        public int CreatedCount { get; set; }

        public void Handle(TeamCreated domainEvent)
        {
            CreatedCount = CreatedCount + 1;
        }
    }
}