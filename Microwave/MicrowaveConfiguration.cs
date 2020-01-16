using System;
using System.Linq;
using Microwave.Queries.Ports;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public void WithFeedType(Type feedType)
        {
            var interfaces = feedType.GetInterfaces();
            var feedInterfaceType = typeof(IEventFeed<>);
            var interfaceNames = interfaces.Select(i => i.Name);
            if (!interfaceNames.Contains(feedInterfaceType.Name))
            {
                throw new ProvidedTypeIsNoEventFeedException(feedType);
            }
            FeedType = feedType;
        }
        public Type FeedType { get; private set; }
    }
}