using System.Collections.Generic;
using System.Reflection;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class Entity : IApply
    {
        public void Apply(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var domainEventType = domainEvent.GetType();
                var entityType = GetType();

                var interfaces = entityType.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IApply<>)) continue;
                    var genericType = iface.GetGenericArguments()[0];
                    if (genericType != domainEventType) continue;
                    var correctApplyMethod = iface.GetMethod(nameof(IApply.Apply), BindingFlags.Public | BindingFlags.Instance);
                    if (correctApplyMethod != null) correctApplyMethod.Invoke(this, new object[] {domainEvent});
                }
            }
        }
    }
}