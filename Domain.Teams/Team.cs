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
        public Guid RaceId { get; private set; }

        public GoldCoins TeamMoney { get; private set; } = new GoldCoins(1000000);
        public IEnumerable<PlayerPosition> Players { get; } = new List<PlayerPosition>();

        public static DomainResult Create(Guid raceId, string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), raceId, teamName, trainerName));
        }

        public DomainResult BuyPlayer(PlayerPosition player)
        {
            if (player.Cost.LessThan(TeamMoney))
            {
                Players.Append(player);
                TeamMoney = TeamMoney.Minus(player.Cost);
                var playerId = Guid.NewGuid();
                return DomainResult.Ok(new PlayerBought(Id, playerId, player.TypeId, player.Cost.Value));
            }

            return DomainResult.Error(new []{$"Can not buy Player. Player costs {player.Cost}, your chest only contains {TeamMoney.Value}" });
        }

        public void Apply(TeamCreated teamCreated)
        {
            Id = teamCreated.EntityId;
            RaceId = teamCreated.RaceId;
        }

        public void Apply(PlayerBought playerBought)
        {
            TeamMoney = new GoldCoins(TeamMoney.Value - playerBought.PlayerCost);

        }

        public int PlayersOfTypeAmount(Guid playerTypeId)
        {
            return Players.Count(player => player.TypeId == playerTypeId);
        }
    }
}