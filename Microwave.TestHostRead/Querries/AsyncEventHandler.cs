using System;
using System.Threading.Tasks;
using Microwave.Queries;
using Microwave.TestHostRead.DomainEvents;

namespace Microwave.TestHostRead.Querries
{
    public class AsyncEventHandler : IHandleAsync<TeamCreated>
    {
        public Task HandleAsync(TeamCreated domainEvent)
        {
            Console.WriteLine("dasda");
            return Task.CompletedTask;
        }
    }
}