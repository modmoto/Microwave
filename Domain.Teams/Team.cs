using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class Team : Entity
    {
        public Race Race { get; }
        public GoldCoins TeamMoney { get; private set; }
        public IEnumerable<Guid> Players { get; } = new List<Guid>();

        public static DomainResult Create(string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), teamName, trainerName));
        }

        public DomainResult BuyPlayer(PlayerPosition player)
        {
            var domainErrors = new List<string>();
            var canUsePlayer = Race.CanUsePlayer(player.PlayerId);
            if (canUsePlayer.Failed) domainErrors.AddRange(canUsePlayer.DomainErrors);

            if (player.Cost.LessThan(TeamMoney))
            {
                Players.Append(player.PlayerId);
                TeamMoney = TeamMoney.Minus(player.Cost);
                return DomainResult.Ok(new PlayerBought(Id, player.PlayerId, player.TypeId, player.Cost.Value));
            }

            domainErrors.Add($"Can not buy Player. Player costs {player.Cost}, your chest only contains {TeamMoney.Value}" );
            return DomainResult.Error(domainErrors);
        }
    }

    public class Race : Entity
    {
        public string Name { get; }
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; }

        public DomainResult CanUsePlayer(Guid playerTypeId)
        {
            var foundPosition = AllowedPlayers.FirstOrDefault(ap => ap.PlayerTypeId == playerTypeId);
            return foundPosition == null ? DomainResult.Error(new List<string> { $"Can not use playertyp: {playerTypeId} in this Race."} ) : DomainResult.Ok(new List<DomainEvent>());
        }
    }
}