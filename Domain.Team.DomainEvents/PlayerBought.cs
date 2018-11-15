using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class PlayerBought : DomainEvent
    {
        public Guid PlayerId { get; }
        public int PlayerCost { get; }
        public Guid PlayerTypeId { get; }

        public PlayerBought(Guid teamId, Guid playerId, Guid playerTypeId, int playerCost) : base(teamId)
        {
            PlayerId = playerId;
            PlayerCost = playerCost;
            PlayerTypeId = playerTypeId;
        }
    }
}