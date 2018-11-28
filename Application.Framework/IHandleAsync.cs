using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.Application
{
    public interface IHandleAsync<in T> where T : IDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }

    public interface IHandle<in T> where T : IDomainEvent
    {
        void Handle(T domainEvent);
    }
}