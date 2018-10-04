using System;
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

        public void Handle(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }
    }
}