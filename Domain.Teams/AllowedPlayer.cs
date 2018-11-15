using System;
using System.Collections.Generic;
using Domain.Framework;

namespace Domain.Teams
{
    public class AllowedPlayer
    {
        public AllowedPlayer(Guid playerTypeId, string type, int minimumPlayers, int maxmimumPlayers)
        {
            PlayerTypeId = playerTypeId;
            MinimumPlayers = minimumPlayers;
            MaxmimumPlayers = maxmimumPlayers;
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
        public string Type { get; }
        public int MinimumPlayers { get; }
        public int MaxmimumPlayers { get; }
    }
}