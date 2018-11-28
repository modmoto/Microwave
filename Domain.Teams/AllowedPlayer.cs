using System;
using System.Collections.Generic;
using Domain.Teams.DomainErrors;
using Microwave.Domain;

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

            if (!moreThanMax) return DomainResult.Error(new TeamFullError(MaxmimumPlayers));

            return DomainResult.Ok(new List<IDomainEvent>());
        }

        public Guid PlayerTypeId { get; }
        public int MaxmimumPlayers { get; }
        public GoldCoins Cost { get; }
    }
}