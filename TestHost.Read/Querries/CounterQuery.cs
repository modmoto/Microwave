using Microwave.Queries;
using TestHost.Read.DomainEvents;

namespace TestHost.Read.Querries
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