using System.Linq;
using System.Reflection;
using Microwave.Domain;

namespace Microwave.Queries
{
    public class Query
    {
        public void Handle(IDomainEvent domainEvent)
        {
            var domainEventType = domainEvent.GetType();
            var entityType = GetType();
            var privateAndPublicMethods = entityType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var applyMethods = privateAndPublicMethods.Where(m => m.Name == nameof(Handle) && m.GetParameters().Length == 1);
            var correctApplyMethod = applyMethods.FirstOrDefault(method => method.GetParameters().Single().ParameterType == domainEventType);
            if (correctApplyMethod != null) correctApplyMethod.Invoke(this, new object[] {domainEvent});
        }
    }

    public class ReadModel : Query
    {
    }
}