using System;
using System.Linq;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EventSourcingApplyStrategy : IEventSourcingStrategy
    {
        public T Apply<T>(T entity, DomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            var currentEntityType = entity.GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == "Apply");
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            if (methodToExecute == null || methodToExecute.GetParameters().Length != 1) return entity;
            methodToExecute.Invoke(entity, new object[] {domainEvent});
            return entity;
        }

        public void SetId<T>(T entity, Guid commandEntityId)
        {
        }
    }
}