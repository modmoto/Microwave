using System;

namespace Domain.PlayerAquisitions
{
    public class Race
    {
        public Race(Guid id, string name)
        {
            Name = name;
            Id = id;
        }

        public Guid Id { get; }
        public string Name { get; }
    }
}