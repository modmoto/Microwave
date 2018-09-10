namespace Application.Framework.Results
{
    public class EventStoreResult
    {
        public long EntityVersion { get; }

        private EventStoreResult(long entityVersion)
        {
            EntityVersion = entityVersion;
        }

        public static EventStoreResult Ok(long entityVersion)
        {
            return new EventStoreResult(entityVersion);
        }
    }

    public class EventStoreResult<T>
    {
        public T Result { get; }
        public long EntityVersion { get; }

        private EventStoreResult(T result, long entityVersion)
        {
            Result = result;
            EntityVersion = entityVersion;
        }

        public static EventStoreResult<T> Ok(T domainEvents, long entityVersion)
        {
            return new EventStoreResult<T>(domainEvents, entityVersion);
        }
    }
}