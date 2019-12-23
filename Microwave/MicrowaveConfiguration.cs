using System;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public void WithFeedType(Type feedType)
        {
            FeedType = feedType;
        }
        public Type FeedType { get; private set; }
    }
}