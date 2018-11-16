using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Team.DomainEvents;

namespace Domain.Teams
{
    public class Race
    {
        public Race()
        {
        }

        public IEnumerable<AllowedPlayer> AllowedPlayers { get; } = new List<AllowedPlayer>();
        public Guid Id { get; }

        public void Apply(AllowedPlayerTypeBalanced playerTypeBalanced)
        {
            var player = AllowedPlayers.First(type => type.Type == playerTypeBalanced.PlayerTypeId);
            player.Apply(playerTypeBalanced);
        }

        public void Apply(RaceCreated raceCreated)
        {

        }
    }
}