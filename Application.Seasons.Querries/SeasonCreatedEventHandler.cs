using System;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonCreatedEventHandler : IHandleAsync<SeasonCreatedEvent>, IHandleAsync<SeasonNameChangedEvent>
    {
        public async Task HandleAsync(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }

        public async Task HandleAsync(SeasonNameChangedEvent domainEvent)
        {
            Console.WriteLine("Rofl");
        }
    }

    public class SeasonChangedNamEventHandler : IHandleAsync<SeasonNameChangedEvent>
    {
        public async Task HandleAsync(SeasonNameChangedEvent domainEvent)
        {
            Console.WriteLine($"New Name: {domainEvent.Name}");
        }
    }
}