namespace Adapters.Framework.EventStores
{
    public abstract class EventStoreConfig
    {
        public abstract string EventStream { get; }
        public abstract string EntityStream { get; }
    }

    public class RealEventStoreConfig : EventStoreConfig
    {
        public override string EventStream => "SeasonEvents";
        public override string EntityStream => "Season";
    }

    public class TestEventStoreConfig : EventStoreConfig
    {
        public override string EventStream => "SeasonEventsINT";
        public override string EntityStream => "SeasonINT";
    }
}