using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IHandleAsync<in T> where T : DomainEvent
    {
        Task Handle(T domainEvent);
    }

    public interface IHandle<in T> where T : DomainEvent
    {
        void Handle(T domainEvent);
    }
}