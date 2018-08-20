using System;
using Domain.Framework;

namespace Domain.Seasons.Events
{
    public class SeasonDateChangedEvent : DomainEvent
    {
        public DateTimeOffset StartDate { get; }
        public DateTimeOffset EndDate { get; }

        public SeasonDateChangedEvent(Guid id, DateTimeOffset startDate, DateTimeOffset endDate) : base(id)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}