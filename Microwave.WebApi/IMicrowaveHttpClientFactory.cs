using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microwave.WebApi
{
    public interface IMicrowaveHttpClientFactory
    {
        Task<HttpClient> CreateHttpClient(Uri serviceAddress);
    }
}