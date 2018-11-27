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
        public IEnumerable<Guid> PlayersTypes { get; } = new List<Guid>();
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; } = new List<AllowedPlayer>();

        public static DomainResult Create(Guid raceId, string teamName, string trainerName)
        {
            return DomainResult.Ok(new TeamCreated(Guid.NewGuid(), raceId, teamName, trainerName));
        }

        public DomainResult BuyPlayer(Guid playerTypeId)
        {
            var play = AllowedPlayers.FirstOrDefault(ap => ap.PlayerTypeId == playerTypeId);
            if (play == null) return DomainResult.Error(new CanNotUsePlayerInThisRaceError(playerTypeId, RaceId));
            int ammount = PlayersTypes.Count(p => p == playerTypeId);

            var canUsePlayer = play.CanUsePlayer(ammount);
            if (canUsePlayer.Failed) return DomainResult.Error(canUsePlayer.DomainErrors);

            if (play.Cost.LessThan(TeamMoney))
            {
                PlayersTypes.Append(playerTypeId);
                TeamMoney = TeamMoney.Minus(play.Cost);
                var playerBought = new PlayerBought(Id, playerTypeId, play.Cost.Value);
                Apply(playerBought);
                return DomainResult.Ok(playerBought);
            }

            return DomainResult.Error(new FewMoneyInTeamChestError(play.Cost.Value, TeamMoney.Value));
        }

        public void Apply(TeamCreated teamCreated)
        {
            Id = teamCreated.EntityId;
            RaceId = teamCreated.RaceId;
        }

        public void Apply(PlayerBought playerBought)
        {
            TeamMoney = new GoldCoins(TeamMoney.Value - playerBought.PlayerCost);
            PlayersTypes.Append(playerBought.PlayerTypeId);
        }
    }
}