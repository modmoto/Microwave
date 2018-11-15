using System.Collections.Generic;
using Domain.Framework;

namespace Domain.PlayerAquisitions
{
    public class PlayerAquisition
    {
        public PlayerAquisition(int strength, int agility, int armor, int movement, int goldCosts,
            IEnumerable<Race> allowedRaces, IEnumerable<Skill> skills)
        {
            Strength = strength;
            Agility = agility;
            Armor = armor;
            Movement = movement;
            GoldCosts = goldCosts;
            AllowedRaces = allowedRaces;
            Skills = skills;
        }

        public int Strength { get; }
        public int Agility { get; }
        public int Armor { get; }
        public int Movement { get; }
        public int GoldCosts { get; }
        public IEnumerable<Race> AllowedRaces { get; }
        public IEnumerable<Skill> Skills { get; }
    }
}