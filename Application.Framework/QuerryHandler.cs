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

        public async Task Handle(DomainEvent createdEvent)
        {
            QuerryObject.Apply(createdEvent);
            await _objectPersister.Save(QuerryObject);
        }
    }
}