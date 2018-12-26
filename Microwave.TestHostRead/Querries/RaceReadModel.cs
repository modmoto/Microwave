using System.Collections.Generic;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.TestHostRead.DomainEvents;

namespace Microwave.TestHostRead.Querries
{
    [CreateReadmodelOn(typeof(RaceCreated))]
    public class RaceReadModel : ReadModel, IHandle<RaceCreated>
    {
        public Identity RaceId { get; set; }
        public string RaceDescription { get; set; }
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; set; }

        public void Handle(RaceCreated domainEvent)
        {
            RaceId = domainEvent.EntityId;
            AllowedPlayers = domainEvent.AllowedPlayers;
            RaceDescription = domainEvent.RaceDescription;
        }
    }
}