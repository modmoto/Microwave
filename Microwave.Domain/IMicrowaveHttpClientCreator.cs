using System.Net.Http;

namespace Microwave.Domain
{
    public interface IMicrowaveHttpClientCreator
    {
        HttpClient CreateHttpClient();
    }
}