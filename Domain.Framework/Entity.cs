using System;

namespace Domain.Framework
{
    public class Entity
    {
        public Guid Id { get; protected set; }
        public long Version { get; set; }
    }
}