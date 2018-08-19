using System;
using System.Linq;

namespace Domain.Framework
{
    public abstract class Entity
    {
        public Guid Id { get; set; }

        public void Apply(DomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            var currentEntityType = GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Apply));
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            if (methodToExecute == null || methodToExecute.GetParameters().Length != 1) return;
            methodToExecute.Invoke(this, new object[] {domainEvent});
        }
    }

    public abstract class DomainEvent
    {
        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public Guid EntityId { get; }
    }
}