using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Teams.DomainEvents;

namespace Domain.Teams
{
    public class RaceDto
    {
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; private set; } = new List<AllowedPlayer>();
        public Guid Id { get; private set; }

        public void Apply(RaceCreated raceCreated)
        {
            Id = raceCreated.EntityId;
            AllowedPlayers = raceCreated.AllowedPlayers.Select(p =>
                new AllowedPlayer(p.PlayerTypeId, p.MaximumPlayers, new GoldCoins(p.Cost)));
        }
    }
}