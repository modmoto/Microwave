using System;
using System.Threading.Tasks;
using Microwave.Queries;
using TestHost.Read.DomainEvents;

namespace TestHost.Read.Querries
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