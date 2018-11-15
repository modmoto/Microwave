using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Domain.Teams
{
    public class Race
    {
        private static IEnumerable<Race> Races = new List<Race>
        {
            new Race(new Guid("3134CD83-A308-406B-A348-8D7C6B5452E2"), "Night Elves", new List<AllowedPlayer>
            {
                new AllowedPlayer(new Guid("01BFB1DF-86E3-425E-B1E7-E722EA86A3FF"), "Blitzer", 0, 4),
                new AllowedPlayer(new Guid("6C799F0E-B04B-455C-ACA1-FF79D447D4AE"), "Witchelve", 0, 2),
                new AllowedPlayer(new Guid("FA5F9B0D-8301-4DF7-BBD7-8C784974F332"), "Assassine", 0, 2),
                new AllowedPlayer(new Guid("E1FE5229-73FB-497E-8FA0-DF967F1ED3FE"), "Lineman", 0, 12),

            })
        };

        public static Race Create(Guid raceGuid)
        {
            var race = Races.SingleOrDefault(r =>r.Id == raceGuid);
            if (race == null) throw new DomainValidationException(new List<string> { $"Did not find Race: {raceGuid}"});
            return race;
        }

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

        public DomainResult CanUsePlayer(Guid playerTypeId, int ammount)
        {
            var foundPosition = AllowedPlayers.FirstOrDefault(ap => ap.PlayerTypeId == playerTypeId);
            if (foundPosition == null)
                return DomainResult.Error(new List<string> {$"Can not use playertyp: {playerTypeId} in this Race."});
            var canUsePlayer = foundPosition.CanUsePlayer(ammount);
            if (canUsePlayer.Failed) return DomainResult.Error(canUsePlayer.DomainErrors);
            return DomainResult.Ok(new List<DomainEvent>());
        }
    }
}