using System;

namespace Domain.Framework
{
    public class Entity
    {
        // TODO all private
        public Guid Id { get; set; }
        public long Version { get; set; }
    }
}