using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class TeamCreated : DomainEvent
    {
        public Guid RaceId { get; }
        public string TeamName { get; }
        public string TrainerName { get; }

        public TeamCreated(Guid newGuid, Guid raceId, string teamName, string trainerName) : base(newGuid)
        {
            RaceId = raceId;
            TeamName = teamName;
            TrainerName = trainerName;
        }
    }
}