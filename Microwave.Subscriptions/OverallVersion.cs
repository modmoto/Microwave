using System;

namespace Microwave.Subscriptions
{
    public class OverallVersion
    {
        public Uri ServiceUri { get; }
        public DateTimeOffset NewVersion { get; }

        public OverallVersion(Uri serviceUri, DateTimeOffset newVersion)
        {
            ServiceUri = serviceUri;
            NewVersion = newVersion;
        }
    }
}