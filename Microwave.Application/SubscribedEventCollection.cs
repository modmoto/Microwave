using System.Collections.Generic;

namespace Microwave.Application
{
    public class SubscribedEventCollection
    {
        public SubscribedEventCollection(IEnumerable<string> handleAsyncEvents)
        {
            IHandleAsyncEvents = handleAsyncEvents;
        }

        public IEnumerable<string> IHandleAsyncEvents { get; }
    }
}