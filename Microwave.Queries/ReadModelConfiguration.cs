using System;

namespace Microwave.Queries
{
    public class ReadModelConfiguration
    {
        public Uri DefaultDomainEventLocation { get; }

        public ReadModelConfiguration(Uri defaultDomainEventLocation = null)
        {
            DefaultDomainEventLocation = defaultDomainEventLocation;
        }
        public ReadDatabaseConfig Database { get; set; } = new ReadDatabaseConfig();
        public ReadModelConfig ReadModelConfig { get; set; } = new ReadModelConfig();

        public Uri GetReadModelLocation(Type readModelType)
        {
            return ReadModelConfig.TryGet(readModelType) ?? DefaultDomainEventLocation;
        }
    }
}