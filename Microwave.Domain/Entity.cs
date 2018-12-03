using System.Collections.Generic;
using System.Linq;

namespace Microwave.Domain
{
    public class Entity : IApply
    {
        public void Apply(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var type = domainEvent.GetType();
                var currentEntityType = GetType();
                var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Apply));
                var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
                if (methodToExecute != null && methodToExecute.GetParameters().Length == 1)
                {
                    methodToExecute.Invoke(this, new object[] {domainEvent});
                }
            }
        }
    }
}