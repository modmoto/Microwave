using System.Threading.Tasks;
using Microwave.Discovery;

namespace Microwave.WebApi.Querries
{
    public interface IDomainEventClientFactory
    {
        Task<DomainEventClient<T>> GetClient<T>();
    }

    public class DomainEventClientFactory : IDomainEventClientFactory
    {
        private readonly IStatusRepository _statusRepository;

        public DomainEventClientFactory(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        public async Task<DomainEventClient<T>> GetClient<T>()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            var domainEventClient = new DomainEventClient<T>(eventLocation);
            return domainEventClient;
        }
    }
}