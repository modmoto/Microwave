namespace Microwave.EventStores.Ports
{
    public class EventStoreResult<T>
    {
        public EventStoreResult(T entity, long version)
        {
            Version = version;
            Entity = entity;
        }

        public long Version { get; }

        public T Entity { get; }
    }
}