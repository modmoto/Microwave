using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class Team
    {
        public Guid Id { get; private set; }
        public Race Race { get; private set; }

        public GoldCoins TeamMoney { get; private set; } = new GoldCoins(1000000);
        public IEnumerable<PlayerPosition> Players { get; } = new List<PlayerPosition>();

        public static DomainResult Create(Race race, string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), race.Id, teamName, trainerName));
        }

        public DomainResult BuyPlayer(PlayerPosition player)
        {
            var domainErrors = new List<string>();
            var currentAmount = Players.Count(p => p.TypeId == player.TypeId);
            var canUsePlayer = Race.CanUsePlayer(player.TypeId, currentAmount);
            if (canUsePlayer.Failed) domainErrors.AddRange(canUsePlayer.DomainErrors);

            if (player.Cost.LessThan(TeamMoney))
            {
                Players.Append(player);
                TeamMoney = TeamMoney.Minus(player.Cost);
                var playerId = Guid.NewGuid();
                return DomainResult.Ok(new PlayerBought(Id, playerId, player.TypeId, player.Cost.Value));
            }

            domainErrors.Add($"Can not buy Player. Player costs {player.Cost}, your chest only contains {TeamMoney.Value}" );
            return DomainResult.Error(domainErrors);
        }

        public void Apply(TeamCreated teamCreated)
        {
            Id = teamCreated.EntityId;
            Race = Race.Create(teamCreated.RaceId);
        }

        public void Apply(PlayerBought playerBought)
        {
            TeamMoney = new GoldCoins(TeamMoney.Value - playerBought.PlayerCost);

        }

    }
}