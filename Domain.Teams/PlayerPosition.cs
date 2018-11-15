using System;

namespace Domain.Teams
{
    public class PlayerPosition
    {
        public PlayerPosition(GoldCoins cost, Guid typeId)
        {
            Cost = cost;
            TypeId = typeId;
        }

        public GoldCoins Cost { get; }
        public Guid TypeId { get; }
    }
}