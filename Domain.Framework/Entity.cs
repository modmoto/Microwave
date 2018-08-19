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
            var methodToExecute = methodInfos.First(method => method.GetParameters().First().ParameterType == type);
            methodToExecute.Invoke(this, new object[] {domainEvent});
        }
    }

    public abstract class DomainEvent
    {
        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid Id { get; }
        public Guid EntityId { get; }
    }
}