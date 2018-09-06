using System;

namespace Application.Seasons.Commands
{
    public class ChangeNameCommand
    {
        public Guid EntityId { get; set; }
        public string Name { get; set; }
    }
}