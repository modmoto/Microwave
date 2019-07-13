using System.Net.Http;
using Microwave.Domain;

namespace Microwave
{
    public class DefaultMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }
    }
}