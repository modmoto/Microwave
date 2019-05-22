using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IHandleAsync<in T> where T : ISubscribedDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}