using Microwave.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace Microwave.EventStores
{
    public class SnapShotDbo
    {
        [BsonId]
        public string EntityId { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
    }

    public class DomainEventDbo
    {
        public DomainEventKey Key { get; set; }

        [BsonId]
        public string KeyHack {
            get => $"{Key.EntityId}_{Key.Version}";
            set => Key = new DomainEventKey
            {
                EntityId = value.Split('_')[0],
                Version = long.Parse(value.Split('_')[1])
            };
        }
        public IDomainEvent Payload { get; set; }
        public long Created { get; set; }
        public string EventType { get; set; }
    }

    public class DomainEventKey
    {
        public string EntityId { get; set; }
        public long Version { get; set; }
    }
}