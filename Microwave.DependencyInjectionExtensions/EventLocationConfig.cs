using System;
using Microsoft.Extensions.Configuration;
using Microwave.Application;

namespace Microwave.DependencyInjectionExtensions
{
    public class EventLocationConfig : IEventLocationConfig
    {
        private readonly IConfiguration _configuration;

        public EventLocationConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentException("There is no config file in the solution. Add an appsettings.json!");
            _configuration = configuration;
            var baseUrl = _configuration["ServiceBaseURl"];
            if (baseUrl == null) throw new ArgumentException("Baseurl for event feed not defined in appsettings.json.");
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