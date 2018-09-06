using System;

namespace Application.Seasons.Commands
{
    public class ChangeDateCommand
    {
        public Guid EntityId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}