using System;
using System.Collections.Generic;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class AllowedPlayer
    {
    public AllowedPlayer(Guid playerTypeId, int maxmimumPlayers, GoldCoins cost)
        {
            PlayerTypeId = playerTypeId;
            MaxmimumPlayers = maxmimumPlayers;
            Cost = cost;
        }

        public DomainResult CanUsePlayer(int ammount)
        {
            var moreThanMax = ammount < MaxmimumPlayers;

            if (!moreThanMax) return DomainResult.Error(new List<string> { $"Can not add more players than {MaxmimumPlayers} for the PlayerType in this Race."} );

            return DomainResult.Ok(new List<IDomainEvent>());
        }

        public Guid PlayerTypeId { get; }
        public int MaxmimumPlayers { get; private set; }
        public GoldCoins Cost { get; }
    }
}