using System.Collections.Generic;
using Microwave.Domain;

namespace Microwave.TestHostWrite.Domain.DomainEvents
{
    public class TeamCreated : IDomainEvent
    {
        public TeamCreated(Identity entityId, StringIdentity raceId, string teamName, string trainerName, IEnumerable<AllowedPlayer> allowedPlayers)
        {
            EntityId = entityId;
            RaceId = raceId;
            TeamName = teamName;
            TrainerName = trainerName;
            AllowedPlayers = allowedPlayers;
        }

        public Identity EntityId { get; }
        public StringIdentity RaceId { get; }
        public string TeamName { get; }
        public string TrainerName { get; }
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; }
    }
}