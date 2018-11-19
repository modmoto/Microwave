using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class PlayerBought : IDomainEvent
    {
        public int PlayerCost { get; }
        public Guid PlayerTypeId { get; }

        public PlayerBought(Guid teamId, Guid playerTypeId, int playerCost)
        {
            EntityId = teamId;
            PlayerCost = playerCost;
            PlayerTypeId = playerTypeId;
        }

        public Guid EntityId { get; }
    }
}