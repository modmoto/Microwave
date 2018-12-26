using System.Collections.Generic;
using Microwave.Domain;

namespace Microwave.TestHostWrite.Domain.DomainEvents
{
    public class RaceCreated : IDomainEvent
    {
        public RaceCreated(Identity entityId, IEnumerable<AllowedPlayer> allowedPlayers, string raceDescription)
        {
            EntityId = entityId;
            AllowedPlayers = allowedPlayers;
            RaceDescription = raceDescription;
        }

        public IEnumerable<AllowedPlayer> AllowedPlayers { get; }
        public string RaceDescription { get; }
        public Identity EntityId { get; }
    }
}