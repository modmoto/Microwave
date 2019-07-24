using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microwave.WebApi
{
    public class DefaultMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public Task<HttpClient> CreateHttpClient(Uri serviceAddress)
        {
            return Task.FromResult(new HttpClient());
        }
    }
}