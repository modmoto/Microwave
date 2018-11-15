using System;

namespace Domain.Teams
{
    public class PlayerPosition
    {
        public PlayerPosition(Guid playerId, GoldCoins cost, Guid typeId)
        {
            PlayerId = playerId;
            Cost = cost;
            TypeId = typeId;
        }

        public Guid PlayerId { get; }
        public GoldCoins Cost { get; }
        public Guid TypeId { get; }
    }
}