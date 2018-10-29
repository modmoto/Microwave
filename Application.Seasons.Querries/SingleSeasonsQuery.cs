using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SingleSeasonsQuery : IdentifiableQuery, IHandle<SeasonCreatedEvent>, IHandle<SeasonNameChangedEvent>
    {
        public string Name { get; set; }

        public void Handle(SeasonCreatedEvent domaintEvent)
        {
            Id = domaintEvent.EntityId;
            Name = domaintEvent.InitialName;
        }

        public void Handle(SeasonNameChangedEvent nameChangedEvent)
        {
            Name = nameChangedEvent.Name;
        }
    }
}