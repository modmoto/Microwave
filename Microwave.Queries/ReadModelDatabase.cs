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