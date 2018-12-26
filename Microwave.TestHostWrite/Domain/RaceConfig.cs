using System.Collections.Generic;
using Microwave.Domain;
using Microwave.TestHostWrite.Domain.DomainEvents;

namespace Microwave.TestHostWrite.Domain
{
    public class RaceConfig : Entity
    {
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; private set; } = new List<AllowedPlayer>();
        public StringIdentity Id { get; private set; }

        public void Apply(RaceCreated raceCreated)
        {
            Id = (StringIdentity) raceCreated.EntityId;
            AllowedPlayers = raceCreated.AllowedPlayers;
        }
    }
}