using System;
using System.Collections.Generic;
using Microwave.Domain;

namespace Domain.Teams.DomainEvents
{
    public class RaceCreated : IDomainEvent
    {
        public RaceCreated(Guid entityId)
        {
            EntityId = entityId;
        }

        public IEnumerable<AllowedPlayerDto> AllowedPlayers { get; } = new List<AllowedPlayerDto>();


        public class AllowedPlayerDto
        {
            public AllowedPlayerDto(Guid playerTypeId, int maximumPlayers, int cost)
            {
                PlayerTypeId = playerTypeId;
                MaximumPlayers = maximumPlayers;
                Cost = cost;
            }

            public Guid PlayerTypeId { get; }
            public int MaximumPlayers { get; }
            public int Cost { get; }
        }

        public Guid EntityId { get; }
    }
}