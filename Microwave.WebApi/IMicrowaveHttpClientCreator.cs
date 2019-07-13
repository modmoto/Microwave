using System;
using System.Net.Http;

namespace Microwave.WebApi
{
    public interface IMicrowaveHttpClientCreator
    {
        HttpClient CreateHttpClient(Uri serviceAddress);
    }
}