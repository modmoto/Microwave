using System;
using System.Linq;

namespace Domain.Framework
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
    }
}