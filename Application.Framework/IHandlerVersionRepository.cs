using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IHandlerVersionRepository
    {
        Task<long> GetLastProcessedVersion(IReactiveEventHandler eventHandler, string eventName);
        void IncrementProcessedVersion(IReactiveEventHandler eventHandler, DomainEvent prozessedEvent,
            StreamVersion streamVersion);
    }
}