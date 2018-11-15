using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class PlayerBought : DomainEvent
    {
        public Guid PlayerId { get; }
        public int PlayerCost { get; }
        public int PlayerTypeKey { get; }

        public PlayerBought(Guid teamId, Guid playerId, int playerCost, int playerTypeKey) : base(teamId)
        {
            PlayerId = playerId;
            PlayerCost = playerCost;
            PlayerTypeKey = playerTypeKey;
        }
    }
}