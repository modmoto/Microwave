using System.Collections.Generic;
using Microwave.Application.Discovery;

namespace Microwave.Application
{
    public class SubscribedEventCollection
    {
        public SubscribedEventCollection(
            IEnumerable<EventSchema> events,
            IEnumerable<ReadModelSubscription> readModelSubcriptions)
        {
            Events = events;
            ReadModelSubcriptions = readModelSubcriptions;
        }

        public IEnumerable<EventSchema> Events { get; }
        public IEnumerable<ReadModelSubscription> ReadModelSubcriptions { get; }
    }

    public class ReadModelSubscription
    {
        public ReadModelSubscription(string readModelName, EventSchema getsCreatedOn)
        {
            ReadModelName = readModelName;
            GetsCreatedOn = getsCreatedOn;
        }

        public string ReadModelName { get; }
        public EventSchema GetsCreatedOn { get; }

        public override bool Equals(object obj)
        {
            if (obj is ReadModelSubscription rms)
            {
                return rms.ReadModelName == ReadModelName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ReadModelName != null ? ReadModelName.GetHashCode() : 0;
        }
    }
}