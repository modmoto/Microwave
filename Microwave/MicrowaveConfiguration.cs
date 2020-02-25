using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.EventStores.SnapShots;
using Microwave.Logging;
using Microwave.Queries.Polling;
using Microwave.Queries.Ports;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public MicrowaveConfiguration WithFeedType(Type feedType)
        {
            var interfaces = feedType.GetInterfaces();
            var feedInterfaceType = typeof(IEventFeed<>);
            var interfaceNames = interfaces.Select(i => i.Name);
            if (!interfaceNames.Contains(feedInterfaceType.Name))
            {
                throw new ProvidedTypeIsNoEventFeedException(feedType);
            }
            FeedType = feedType;
            return this;
        }

        public MicrowaveConfiguration WithLogLevel(MicrowaveLogLevel logLevel)
        {
            LogLevel = new MicrowaveLogLevelType(logLevel);
            return this;
        }


        public IList<ISnapShot> SnapShots { get; } = new List<ISnapShot>();
        public IList<IPollingInterval> PollingIntervals { get; } = new List<IPollingInterval>();

        public Type FeedType { get; private set; } = typeof(LocalEventFeed<>);
        public MicrowaveLogLevelType LogLevel { get; private set; } = new MicrowaveLogLevelType(MicrowaveLogLevel.None);
    }
}