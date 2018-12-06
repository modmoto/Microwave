using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IHandleAsync<in T> where T : IDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}