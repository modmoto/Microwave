using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public class QuerryHandler<T> where T : Querry
    {
        protected readonly T QuerryObject;

        public QuerryHandler(T querryObject)
        {
            QuerryObject = querryObject;
        }

        public void Handle(DomainEvent domainEvent)
        {
            QuerryObject.Apply(domainEvent);
        }

        public void Handle(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                QuerryObject.Apply(domainEvent);
            }
        }
    }
}