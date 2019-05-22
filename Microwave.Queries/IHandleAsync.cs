using System.Threading.Tasks;
using Microwave.Application;

namespace Microwave.Queries
{
    public interface IHandleAsync<in T> where T : ISubscribedDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}