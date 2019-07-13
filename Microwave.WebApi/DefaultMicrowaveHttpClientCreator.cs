using System;
using System.Net.Http;

namespace Microwave.WebApi
{
    public class DefaultMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public HttpClient CreateHttpClient(Uri serviceAddress)
        {
            return new HttpClient();
        }
    }
}