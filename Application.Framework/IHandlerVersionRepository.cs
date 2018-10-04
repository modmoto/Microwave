using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IHandlerVersionRepository
    {
        Task<long> GetLastProcessedVersion(IEventHandler eventHandler, string eventName);
        void IncrementProcessedVersion(IEventHandler eventHandler, DomainEvent prozessedEvent);
    }
}