using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IHandleAsync<in T>
    {
        Task HandleAsync(T domainEvent);
    }
}