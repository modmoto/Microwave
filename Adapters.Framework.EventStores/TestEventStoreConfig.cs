namespace Adapters.Framework.EventStores
{
    public abstract class EventStoreConfig
    {
        public abstract string ReadStream { get; }
        public abstract string WriteStream { get; }
    }

    public class RealEventStoreConfig : EventStoreConfig
    {
        public override string ReadStream => "SeasonRead";
        public override string WriteStream => "SeasonWrite";
    }

    public class TestEventStoreConfig : EventStoreConfig
    {
        public override string ReadStream => "SeasonReadTest";
        public override string WriteStream => "SeasonWriteTest";
    }
}