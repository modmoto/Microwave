namespace Microwave.Queries.Ports
{
    public class SubscribedDomainEventWrapper
    {
        public string DomainEventType => DomainEvent.GetType().Name;
        public long OverallVersion { get; set; }
        public long EntityStreamVersion { get; set; }
        public ISubscribedDomainEvent DomainEvent { get; set; }
    }
}