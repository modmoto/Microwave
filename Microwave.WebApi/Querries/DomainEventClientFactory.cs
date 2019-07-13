using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries;

namespace Microwave.WebApi.Querries
{
    public interface IDomainEventClientFactory
    {
        Task<HttpClient> GetClient<T>();
    }

    public class DomainEventClientFactory : IDomainEventClientFactory
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IMicrowaveHttpClientCreator _httpClientCreator;

        public DomainEventClientFactory(IStatusRepository statusRepository, IMicrowaveHttpClientCreator httpClientCreator)
        {
            _statusRepository = statusRepository;
            _httpClientCreator = httpClientCreator;
        }

        public async Task<HttpClient> GetClient<T>()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            var domainEventClient = new DomainEventClient<T>(eventLocation);
            return domainEventClient;
        }

        private static HttpClient DefaultHttpClient() => new HttpClient { BaseAddress = null };
    }
}