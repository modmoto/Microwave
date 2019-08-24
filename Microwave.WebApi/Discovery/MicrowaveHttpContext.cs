using System;

namespace Microwave.WebApi.Discovery
{
    public class MicrowaveHttpContext
    {
        private string _rawUri;
        public Uri AppBaseUrl => new Uri(_rawUri);

        public void Configure(string rawUri)
        {
            _rawUri = rawUri;
        }
    }
}