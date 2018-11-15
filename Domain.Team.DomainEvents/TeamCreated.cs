using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class TeamCreated : DomainEvent
    {
        public string TeamName { get; }
        public string TrainerName { get; }

        public TeamCreated(Guid newGuid, string teamName, string trainerName) : base(newGuid)
        {
            TeamName = teamName;
            TrainerName = trainerName;
        }
    }
}