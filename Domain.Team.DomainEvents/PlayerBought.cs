using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class PlayerBought : DomainEvent
    {
        public int PlayerCost { get; }
        public Guid PlayerTypeId { get; }

        public PlayerBought(Guid teamId, Guid playerTypeId, int playerCost) : base(teamId)
        {
            PlayerCost = playerCost;
            PlayerTypeId = playerTypeId;
        }
    }
}