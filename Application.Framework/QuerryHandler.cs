using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Domain.Framework;

namespace Application.Framework
{
    public abstract class QuerryHandler<T> where T : Entity
    {
        private readonly IObjectPersister<T> _objectPersister;

        public QuerryHandler(IObjectPersister<T> objectPersister)
        {
            _objectPersister = objectPersister;
        }

        public async Task Handle(DomainEvent createdEvent)
        {
            var querry = await _objectPersister.GetAsync();
            querry.Apply(createdEvent);
            await _objectPersister.Save(querry);
        }
    }
}