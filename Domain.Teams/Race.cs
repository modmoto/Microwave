using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Domain.Teams
{
    public class Race : Entity
    {
        public Race(string name, IEnumerable<AllowedPlayer> allowedPlayers)
        {
            Name = name;
            AllowedPlayers = allowedPlayers;
        }

        public string Name { get; }
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; }

        public DomainResult CanUsePlayer(Guid playerTypeId, int ammount)
        {
            var foundPosition = AllowedPlayers.FirstOrDefault(ap => ap.PlayerTypeId == playerTypeId);
            if (foundPosition == null)
                return DomainResult.Error(new List<string> {$"Can not use playertyp: {playerTypeId} in this Race."});
            var canUsePlayer = foundPosition.CanUsePlayer(ammount);
            if (canUsePlayer.Failed) return DomainResult.Error(canUsePlayer.DomainErrors);
            return DomainResult.Ok(new List<DomainEvent>());
        }
    }
}