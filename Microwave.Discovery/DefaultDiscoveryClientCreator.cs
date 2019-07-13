using System.Net.Http;

namespace Microwave.Discovery
{
    public interface IMicrowaveHttpClientCreator
    {
        HttpClient CreateHttpClient();
    }

    public class DefaultMicrowaveHttpClientCreator : IMicrowaveHttpClientCreator
    {
        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }
    }
}