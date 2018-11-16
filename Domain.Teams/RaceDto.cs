using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class RaceDto
    {
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; private set; } = new List<AllowedPlayer>();
        public Guid Id { get; private set; }

        public void Apply(AllowedPlayerTypeBalanced playerTypeBalanced)
        {
            var player = AllowedPlayers.First(type => type.PlayerTypeId == playerTypeBalanced.PlayerTypeId);
            player.Apply(playerTypeBalanced);
        }

        public void Apply(RaceCreated raceCreated)
        {
            Id = raceCreated.EntityId;
            AllowedPlayers = raceCreated.AllowedPlayers.Select(p =>
                new AllowedPlayer(p.PlayerTypeId, p.MaximumPlayers, new GoldCoins(p.Cost)));
        }
    }
}