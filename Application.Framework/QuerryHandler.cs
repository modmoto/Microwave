using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QuerryHandler<T> where T : Querry
    {
        protected readonly IObjectPersister<T> ObjectPersister;
        protected readonly T QuerryObject;

        public QuerryHandler(IObjectPersister<T> objectPersister, T querryObject)
        {
            ObjectPersister = objectPersister;
            QuerryObject = querryObject;
        }

        public async Task Handle(DomainEvent createdEvent)
        {
            QuerryObject.Apply(createdEvent);
            await ObjectPersister.Save(QuerryObject);
        }
    }
}