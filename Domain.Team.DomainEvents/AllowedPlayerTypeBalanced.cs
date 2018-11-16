using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class AllowedPlayerTypeBalanced : DomainEvent
    {
        public AllowedPlayerTypeBalanced(Guid raceId, Guid playerTypeId) : base(raceId)
        {
            PlayerTypeId = playerTypeId;
        }

        public Guid PlayerTypeId { get; }
        public int MinimumPlayers { get; set; }
        public int MaxmimumPlayers { get; set; }
    }
}