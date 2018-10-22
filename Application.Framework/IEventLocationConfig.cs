using System;

namespace Application.Framework
{
    public interface IEventLocationConfig
    {
        Uri GetLocationFor(string domainEvent);
    }

    public class EventLocationConfig : IEventLocationConfig
    {
        public Uri GetLocationFor(string domainEvent)
        {
            return new Uri($"http://localhost:5000/Api/DomainEventTypeStreams/{domainEvent}");
        }
    }
}