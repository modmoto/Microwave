using System.Threading.Tasks;
using Domain.Framework;

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