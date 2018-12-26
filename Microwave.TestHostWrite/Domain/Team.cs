using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Domain;
using Microwave.TestHostWrite.Domain.DomainErrors;
using Microwave.TestHostWrite.Domain.DomainEvents;

namespace Microwave.TestHostWrite.Domain
{
    [SnapShotAfter(3)]
    public class Team : Entity
    {
        public Identity Id { get; private set; }

        public GoldCoins TeamMoney { get; private set; } = new GoldCoins(1000000);
        public IEnumerable<Identity> PlayerTypes { get; private set; } = new List<Identity>();
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; private set; } = new List<AllowedPlayer>();

        public static DomainResult Create(StringIdentity raceId, string teamName, string trainerName, IEnumerable<AllowedPlayer>
        allowedPlayers)
        {
            return DomainResult.Ok(new TeamCreated(GuidIdentity.Create(Guid.NewGuid()), raceId, teamName, trainerName, allowedPlayers));
        }

        public DomainResult BuyPlayer(StringIdentity playerTypeId)
        {
            var play = AllowedPlayers.FirstOrDefault(ap => ap.PlayerTypeId == playerTypeId);
            if (play == null) return DomainResult.Error(new CanNotUsePlayerInThisRaceError(playerTypeId));
            int ammount = PlayerTypes.Count(p => p == playerTypeId);

            var canUsePlayer = play.CanUsePlayer(ammount);
            if (canUsePlayer.Failed) return DomainResult.Error(canUsePlayer.DomainErrors);

            if (!play.Cost.LessThan(TeamMoney))
                return DomainResult.Error(new FewMoneyInTeamChestError(play.Cost.Value, TeamMoney.Value));

            PlayerTypes.Append(playerTypeId);
            TeamMoney = TeamMoney.Minus(play.Cost);
            var playerBought = new PlayerBought(Id, playerTypeId, TeamMoney);
            Apply(playerBought);
            return DomainResult.Ok(playerBought);

        }

        private void Apply(TeamCreated teamCreated)
        {
            Id = teamCreated.EntityId;
            AllowedPlayers = teamCreated.AllowedPlayers;
        }

        private void Apply(PlayerBought playerBought)
        {
            TeamMoney = playerBought.NewTeamChestBalance;
            PlayerTypes = PlayerTypes.Append(playerBought.PlayerTypeId);
        }
    }
}