using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQuerryHandler<T> where T : DomainEvent
    {
        Task Handle(T domainEvent);
    }
}