using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class Team : Entity
    {
        public GoldCoins TeamMoney { get; private set; }
        public IEnumerable<Guid> Players { get; } = new List<Guid>();

        public static DomainResult Create(string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), teamName, trainerName));
        }

        public DomainResult BuyPlayer(PlayerPosition player)
        {
            if (player.Cost.LessThan(TeamMoney))
            {
                Players.Append(player.PlayerId);
                TeamMoney = TeamMoney.Minus(player.Cost);
                return DomainResult.Ok(new PlayerBought(Id, player.PlayerId, player.Cost.Value, player.TypeKey));
            }

            return DomainResult.Error(new List<string> { $"Can not buy Player. Player costs {player.Cost}, your chest only contains {TeamMoney.Value}" });
        }
    }
}