using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Domain.Teams
{
    public class Race
    {
        private Race(Guid raceGuid, string name, IEnumerable<AllowedPlayer> allowedPlayers)
        {
            Id = raceGuid;
            Name = name;
            AllowedPlayers = allowedPlayers;
        }

        public Race()
        {
        }

        public string Name { get; }
        public IEnumerable<AllowedPlayer> AllowedPlayers { get; }
        public Guid Id { get; }
    }
}