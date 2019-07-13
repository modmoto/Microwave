using System.Net.Http;
using System.Net.Http.Headers;
using Microwave.Discovery;

namespace ReadService1
{
    public class MyMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public HttpClient CreateHttpClient()
        {
            var discoveryClient = new HttpClient();
            discoveryClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("123");
            return discoveryClient;
        }
    }
}