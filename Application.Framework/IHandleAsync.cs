using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IHandleAsync<T> where T : DomainEvent
    {
        Task Handle(T domainEvent);
    }
}