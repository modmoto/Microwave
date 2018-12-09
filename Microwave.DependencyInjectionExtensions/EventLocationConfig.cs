using System;
using Microsoft.Extensions.Configuration;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions
{
    public class EventLocationConfig : IEventLocationConfig
    {
        private readonly IConfiguration _configuration;

        public EventLocationConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentException("There is no config file in the solution. Add an appsettings.json!");
            _configuration = configuration;
            var baseUrl = _configuration["DefaultDomainEventLocation"];
            if (baseUrl == null) throw new ArgumentException("Baseurl for event feed not defined in appsettings.json.");
            DefaultEventLocation = CleanUrl(baseUrl);
        }

        public Uri GetLocationForDomainEvent(string domainEvent)
        {
            return new Uri($"{LocationFor(domainEvent, "DomainEventLocations")}/Api/DomainEventTypes/{domainEvent}");
        }

        public Uri GetLocationForReadModel(string readModel)
        {
            return new Uri($"{LocationFor(readModel, "DomainEventReadModelLocations")}/Api/DomainEvents");
        }

        private static string CleanUrl(string eventLocation)
        {
            if (eventLocation.EndsWith("/")) return eventLocation.Remove(eventLocation.Length - 1);
            return eventLocation;
        }

        private string LocationFor(string item, string section)
        {
            var configurationSection = _configuration.GetSection(section);
            var eventLocation = configurationSection[item];
            if (string.IsNullOrEmpty(eventLocation)) eventLocation = DefaultEventLocation;
            return CleanUrl(eventLocation);
        }

        public string DefaultEventLocation { get; }
    }
}