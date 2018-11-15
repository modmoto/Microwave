using Domain.Framework;

namespace Domain.PlayerAquisitions
{
    public class Race : Entity
    {
        public Race(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}