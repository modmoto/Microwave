namespace Adapters.Framework.EventStores
{
    public abstract class EventStoreConfig
    {
        public abstract string ReadStream { get; }
        public abstract string WriteStream { get; }
        public abstract string ProcessedEventCounterStream { get; }
    }

    public class RealEventStoreConfig : EventStoreConfig
    {
        public override string ReadStream => "SeasonRead";
        public override string WriteStream => "SeasonWrite";
        public override string ProcessedEventCounterStream => "SeasonProcessedEventCounter";
    }

    public class TestEventStoreConfig : EventStoreConfig
    {
        public override string ReadStream => "SeasonReadTest";
        public override string WriteStream => "SeasonWriteTest";
        public override string ProcessedEventCounterStream => "SeasonProcessedEventCounterTest";
    }
}