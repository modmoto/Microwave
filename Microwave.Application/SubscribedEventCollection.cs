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
        public ReadModelSubscription(string readModelName, string getsCreatedOn)
        {
            ReadModelName = readModelName;
            GetsCreatedOn = getsCreatedOn;
        }

        public string ReadModelName { get; }
        public string GetsCreatedOn { get; }

        public override bool Equals(object obj)
        {
            if (obj is ReadModelSubscription rms)
            {
                return rms.ReadModelName == ReadModelName;
            }

            return false;
        }
    }
}