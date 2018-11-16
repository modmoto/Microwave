using System;
using System.Collections.Generic;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class AllowedPlayer
    {
    public AllowedPlayer(Guid playerTypeId, Guid type, int minimumPlayers, int maxmimumPlayers, GoldCoins cost)
        {
            PlayerTypeId = playerTypeId;
            MinimumPlayers = minimumPlayers;
            MaxmimumPlayers = maxmimumPlayers;
            Cost = cost;
            Type = type;
        }

        public DomainResult CanUsePlayer(int ammount)
        {
            var moreThanMax = ammount < MaxmimumPlayers;
            var lessThanMin = ammount >= MinimumPlayers;

            if (!moreThanMax) return DomainResult.Error(new List<string> { $"Can not add more players than {MaxmimumPlayers} for the PlayerType in this Race."} );
            if (!lessThanMin) return DomainResult.Error(new List<string> { $"Can not add less players than {MinimumPlayers} for the PlayerType in this Race."} );

            return DomainResult.Ok(new List<DomainEvent>());
        }

        public Guid PlayerTypeId { get; }
        public Guid Type { get; }
        public int MinimumPlayers { get; private set; }
        public int MaxmimumPlayers { get; private set; }
        public GoldCoins Cost { get; }

        public void Apply(AllowedPlayerTypeBalanced playerTypeBalanced)
        {
            MaxmimumPlayers = playerTypeBalanced.MaxmimumPlayers;
            MinimumPlayers = playerTypeBalanced.MinimumPlayers;
        }
    }
}