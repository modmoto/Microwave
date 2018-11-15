using System;

namespace Domain.Teams
{
    public class PlayerPosition
    {
        public PlayerPosition(Guid playerId, GoldCoins cost, int typeKey)
        {
            PlayerId = playerId;
            Cost = cost;
            TypeKey = typeKey;
        }

        public Guid PlayerId { get; }
        public GoldCoins Cost { get; }
        public int TypeKey { get; }
    }
}