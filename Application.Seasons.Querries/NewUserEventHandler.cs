using System;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class NewUserEventHandler : ReactiveEventHandler<NewUserEventHandler>
    {
        public NewUserEventHandler(SubscribedEventTypes<NewUserEventHandler> subscribedEventTypes) : base(subscribedEventTypes)
        {
        }

        public void Handle(SeasonCreatedEvent domainEvent)
        {
            Console.WriteLine("THingy called");
        }
    }
}