using System;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonCreatedEventHandler : ReactiveEventHandler<SeasonCreatedEventHandler>
    {
        public SeasonCreatedEventHandler(SubscribedEventTypes<SeasonCreatedEventHandler> subscribedEventTypes,
            IHandlerVersionRepository versionRepository) : base(subscribedEventTypes, versionRepository)
        {
        }

        public async Task Handle(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }

        public async Task Handle(SeasonNameChangedEvent domainEvent)
        {
            Console.WriteLine($"New Name: {domainEvent.Name}");
        }
    }
}