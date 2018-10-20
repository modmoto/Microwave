using System;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonCreatedEventHandler : ReactiveEventHandler<SeasonCreatedEventHandler>, IHandleAsync<SeasonCreatedEvent>
    {
        public SeasonCreatedEventHandler(SubscribedEventTypes<SeasonCreatedEventHandler> subscribedEventTypes,
            IHandlerVersionRepository versionRepository) : base(subscribedEventTypes, versionRepository)
        {
        }

        public async Task Handle(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }
    }

    public class SeasonChangedNamEventHandler : ReactiveEventHandler<SeasonChangedNamEventHandler>, IHandleAsync<SeasonNameChangedEvent>
    {
        public SeasonChangedNamEventHandler(SubscribedEventTypes<SeasonChangedNamEventHandler> subscribedEventTypes,
            IHandlerVersionRepository versionRepository) : base(subscribedEventTypes, versionRepository)
        {
        }

        public async Task Handle(SeasonNameChangedEvent domainEvent)
        {
            Console.WriteLine($"New Name: {domainEvent.Name}");
        }
    }
}