using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microwave.Domain
{
    public class Entity : IApply
    {
        public void Apply(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var domainEventType = domainEvent.GetType();
                var entityType = GetType();
                var privateAndPublicMethods = entityType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                var applyMethods = privateAndPublicMethods.Where(m => m.Name == nameof(Apply) && m.GetParameters().Length == 1);
                var correctApplyMethod = applyMethods.FirstOrDefault(method => method.GetParameters().Single().ParameterType == domainEventType);
                if (correctApplyMethod != null) correctApplyMethod.Invoke(this, new object[] {domainEvent});
            }
        }
    }
}