namespace Adapters.Framework.EventStores
{
    public abstract class EventStoreConfig
    {
        public abstract string EventStream { get; }
    }

    public class RealEventStoreConfig : EventStoreConfig
    {
        public override string EventStream => "MyRealStream";
    }

    public class TestEventStoreConfig : EventStoreConfig
    {
        public override string EventStream => "MyTestStream";
    }
}