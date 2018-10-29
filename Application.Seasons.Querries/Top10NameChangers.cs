using System;
using System.Collections.Generic;
using Application.Framework;

namespace Application.Seasons.Querries
{
    public class Top10NameChangers : Query
    {
        public IEnumerable<SeasonNameChangCounterDto> SeasonCounter { get; set; } = new List<SeasonNameChangCounterDto>();
    }

    public class SeasonNameChangCounterDto
    {
        public SeasonNameChangCounterDto(Guid seasonId, int nameChangesAmount)
        {
            SeasonId = seasonId;
            NameChangesAmount = nameChangesAmount;
        }

        public Guid SeasonId { get; }
        public int NameChangesAmount { get; }
    }
}