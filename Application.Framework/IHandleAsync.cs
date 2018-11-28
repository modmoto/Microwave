using System.Threading.Tasks;
using Microwave.Domain;

namespace Application.Framework
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