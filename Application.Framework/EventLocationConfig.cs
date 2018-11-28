using System;
using Microsoft.Extensions.Configuration;

namespace Microwave.Application
{
    public class EventLocationConfig
    {
        private readonly IConfiguration _configuration;

        public EventLocationConfig(IConfiguration configuration)
        {
            _configuration = configuration;
            var baseUrl = _configuration["ServiceBaseURl"];
            DefaultEventLocation = CleanUrl(baseUrl);
        }

        public Uri GetLocationFor(string domainEvent)
        {
            var configurationSection = _configuration.GetSection("DomainEventLocations");
            var eventLocation = configurationSection[domainEvent];
            if (string.IsNullOrEmpty(eventLocation)) eventLocation = DefaultEventLocation;
            return new Uri($"{CleanUrl(eventLocation)}/Api/DomainEventTypeStreams/{domainEvent}");
        }

        private static string CleanUrl(string eventLocation)
        {
            if (eventLocation.EndsWith("/")) return eventLocation.Remove(eventLocation.Length - 1);
            return eventLocation;
        }

        public string DefaultEventLocation { get; }
    }
}