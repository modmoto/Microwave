using System;

namespace Domain.Framework
{
    public class Entity
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
    }
}