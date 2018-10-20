using System;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonCreatedEventHandler : IHandleAsync<SeasonCreatedEvent>
    {
        public async Task Handle(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }
    }

    public class SeasonChangedNamEventHandler : IHandleAsync<SeasonNameChangedEvent>
    {
        public async Task Handle(SeasonNameChangedEvent domainEvent)
        {
            Console.WriteLine($"New Name: {domainEvent.Name}");
        }
    }
}