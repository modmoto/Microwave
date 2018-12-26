using System.Collections.Generic;
using Microwave.Domain;

namespace TestHost.Read.DomainEvents
{
    public class RaceCreated : IDomainEvent
    {
        public RaceCreated(Identity entityId, IEnumerable<AllowedPlayer> allowedPlayers, string raceDescription)
        {
            EntityId = entityId;
            AllowedPlayers = allowedPlayers;
            RaceDescription = raceDescription;
        }

        public IEnumerable<AllowedPlayer> AllowedPlayers{ get; set; }
        public string RaceDescription{ get; set; }
        public Identity EntityId{ get; set; }
    }
}