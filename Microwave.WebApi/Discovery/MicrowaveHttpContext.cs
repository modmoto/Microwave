using System;

namespace Microwave.WebApi.Discovery
{
    public class MicrowaveHttpContext
    {
        public Uri AppBaseUrl { get; private set; }

        public void Configure(Uri appBaseUri)
        {
            AppBaseUrl = appBaseUri;
        }
    }
}