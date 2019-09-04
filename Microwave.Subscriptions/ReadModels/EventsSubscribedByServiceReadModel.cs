using System.Collections.Generic;

namespace Microwave.Subscriptions.ReadModels
{
    public class EventsSubscribedByServiceReadModel
    {
        public IEnumerable<EventReadModel> Events { get; set; }
    }
}