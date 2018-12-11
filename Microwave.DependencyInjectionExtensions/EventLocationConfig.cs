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
            var configurationSection = _configuration.GetSection("DomainEventLocations");
            var eventLocation = configurationSection[domainEvent];
            if (string.IsNullOrEmpty(eventLocation)) eventLocation = DefaultEventLocation;
            return new Uri($"{CleanUrl(eventLocation)}/Api/DomainEventTypeStreams/{domainEvent}");
        }

        public Uri GetLocationForReadModel(string readModel)
        {
            var configurationSection = _configuration.GetSection("DomainEventReadModelLocations");
            var eventLocation = configurationSection[readModel];
            if (string.IsNullOrEmpty(eventLocation)) eventLocation = DefaultEventLocation;
            return new Uri($"{CleanUrl(eventLocation)}/Api/DomainEvents");
        }

        public string GetCreationEventForReadModel(string readModel)
        {
            var configurationSection = _configuration.GetSection("DomainEventReadModelCreationEvents");
            var creationEventName = configurationSection[readModel];
            return creationEventName;
        }

        private static string CleanUrl(string eventLocation)
        {
            if (eventLocation.EndsWith("/")) return eventLocation.Remove(eventLocation.Length - 1);
            return eventLocation;
        }

        public string DefaultEventLocation { get; }
    }
}