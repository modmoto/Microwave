using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QuerryHandler<T> where T : Querry
    {
        private readonly IObjectPersister<T> _objectPersister;
        protected readonly T QuerryObject;

        public QuerryHandler(IObjectPersister<T> objectPersister, T querryObject)
        {
            _objectPersister = objectPersister;
            QuerryObject = querryObject;
        }

        public async Task Handle(DomainEvent domainEvent)
        {
            QuerryObject.Apply(domainEvent);
            await _objectPersister.Save(QuerryObject);
        }

        public async Task Handle(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                QuerryObject.Apply(domainEvent);
            }

            await _objectPersister.Save(QuerryObject);
        }
    }
}