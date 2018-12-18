using MongoDB.Bson.Serialization.Attributes;

namespace Microwave.Queries
{
    public class IdentifiableQueryDbo
    {
        [BsonId]
        public string Id { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
        public string QueryType { get; set; }
    }

    public class QueryDbo
    {
        [BsonId]
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class LastProcessedVersionDbo
    {
        [BsonId]
        public string EventType { get; set; }
        public long LastVersion { get; set; }
    }
}