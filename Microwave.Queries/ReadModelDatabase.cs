using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }
        public ReadModelDatabase(ReadModelConfiguration config)
        {
            var dbConfig = config.Database;
            var client = new MongoClient(dbConfig.ConnectionString);
            Database = client.GetDatabase(dbConfig.DatabaseName);
        }
    }

    public class ReadModelConfiguration
    {
        public ReadModelConfiguration(Uri defaultDomainEventLocation)
        {
            DefaultDomainEventLocation = defaultDomainEventLocation;
        }

        public ReadDatabaseConfig Database { get; set; } = new ReadDatabaseConfig();
        public Uri DefaultDomainEventLocation { get; }
        public DomainEventConfig DomainEventConfig { get; set; } = new DomainEventConfig();

        public ReadModelConfig ReadModelConfig { get; set; } = new ReadModelConfig();

        public Uri GetDomainEventLocation(Type domainEventType)
        {
            return DomainEventConfig.TryGet(domainEventType) ?? DefaultDomainEventLocation;
        }

        public Uri GetReadModelLocation(Type readModelType)
        {
            return ReadModelConfig.TryGet(readModelType) ?? DefaultDomainEventLocation;
        }
    }

    [Serializable]
    public class ReadModelConfig : Dictionary<Type, Uri>
    {
        public ReadModelConfig()
        {
        }

        protected ReadModelConfig(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Uri TryGet(Type type)
        {
            return ContainsKey(type) ? this[type] : null;
        }
    }

    [Serializable]
    public class DomainEventConfig : Dictionary<Type, Uri>
    {
        public DomainEventConfig()
        {
        }

        protected DomainEventConfig(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Uri TryGet(Type type)
        {
            return ContainsKey(type) ? this[type] : null;
        }
    }

    public class ReadDatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveReadModelDb";
    }
}