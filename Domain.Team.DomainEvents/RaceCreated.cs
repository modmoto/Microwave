using System;
using Domain.Framework;

namespace Domain.Team.DomainEvents
{
    public class RaceCreated : DomainEvent
    {
        public RaceCreated(Guid entityId) : base(entityId)
        {
        }
    }
}