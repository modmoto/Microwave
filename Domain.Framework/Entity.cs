using System;

namespace Domain.Framework
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
    }
}