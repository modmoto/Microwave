using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microwave.WebApi
{
    public class DefaultMicrowaveHttpClientFactory : IMicrowaveHttpClientFactory
    {
        public Task<HttpClient> CreateHttpClient(Uri serviceAddress)
        {
            return Task.FromResult(new HttpClient());
        }
    }
}