using System.Linq;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class EventSourcingApplyStrategy : IEventSourcingStrategy
    {
        public T Apply<T>(T entity, IDomainEvent domainEvent)
        {
            var interfaces = entity.GetType().GetInterfaces();
            var applyInterfaces = interfaces.Where(inte => inte.GetGenericTypeDefinition() == typeof(IApply<>));
            var applyInterfaceForType = applyInterfaces.FirstOrDefault(af => af.GetGenericArguments().Single() == domainEvent.GetType());
            var applyMethod = applyInterfaceForType?.GetMethod(nameof(IApply<IDomainEvent>.Apply));
            applyMethod?.Invoke(entity, new object[] {domainEvent});
            return entity;
        }
    }
}