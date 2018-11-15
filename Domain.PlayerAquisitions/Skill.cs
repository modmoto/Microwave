using System;

namespace Domain.PlayerAquisitions
{
    public class Skill
    {
        public Skill(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
        
        public Guid Id { get; }
        public string Description { get; }
    }
}