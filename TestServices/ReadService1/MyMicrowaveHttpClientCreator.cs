using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microwave.WebApi;

namespace ReadService1
{
    public class MyMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public HttpClient CreateHttpClient(Uri serviceAdress)
        {
            var discoveryClient = new HttpClient();
            discoveryClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("123");
            return discoveryClient;
        }
    }
}