using System.Collections.Generic;

namespace Microwave.Application
{
    public class SubscribedEventCollection
    {
        public SubscribedEventCollection(
            IEnumerable<string> handleAsyncEvents,
            IEnumerable<ReadModelSubscription> readModelSubcriptions)
        {
            IHandleAsyncEvents = handleAsyncEvents;
            ReadModelSubcriptions = readModelSubcriptions;
        }

        public IEnumerable<string> IHandleAsyncEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModelSubcriptions { get; }
    }

    public class ReadModelSubscription
    {
        public ReadModelSubscription(IEnumerable<string> subscribedEvents, string getsCreatedOn)
        {
            SubscribedEvents = subscribedEvents;
            GetsCreatedOn = getsCreatedOn;
        }

        public IEnumerable<string> SubscribedEvents { get; }
        public string GetsCreatedOn { get; }
    }
}