using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class AllowedPlayerTypeBalanced : IDomainEvent
    {
        public AllowedPlayerTypeBalanced(Guid raceId, Guid playerTypeId)
        {
            EntityId = raceId;
            PlayerTypeId = playerTypeId;
        }

        public Guid PlayerTypeId { get; }
        public int MaxmimumPlayers { get; set; }
        public Guid EntityId { get; }
    }
}