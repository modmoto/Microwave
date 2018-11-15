using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class Team
    {
        public Guid Id { get; }
        public Race Race { get; }
        public GoldCoins TeamMoney { get; private set; }
        public IEnumerable<PlayerPosition> Players { get; } = new List<PlayerPosition>();

        public static DomainResult Create(Race race, string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), race.Id, teamName, trainerName));
        }

        public DomainResult BuyPlayer(PlayerPosition player)
        {
            var domainErrors = new List<string>();
            var currentAmount = Players.Count(p => p.TypeId == player.TypeId);
            var canUsePlayer = Race.CanUsePlayer(player.PlayerId, currentAmount);
            if (canUsePlayer.Failed) domainErrors.AddRange(canUsePlayer.DomainErrors);

            if (player.Cost.LessThan(TeamMoney))
            {
                Players.Append(player);
                TeamMoney = TeamMoney.Minus(player.Cost);
                return DomainResult.Ok(new PlayerBought(Id, player.PlayerId, player.TypeId, player.Cost.Value));
            }

            domainErrors.Add($"Can not buy Player. Player costs {player.Cost}, your chest only contains {TeamMoney.Value}" );
            return DomainResult.Error(domainErrors);
        }
    }
}