using System;
using System.Linq;
using Microwave.Domain;

namespace Microwave.Queries
{
    public class Query
    {
        public void Handle(IDomainEvent domainEvent, long version)
        {
            var type = domainEvent.GetType();
            var currentEntityType = GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Handle));
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            if (methodToExecute == null || methodToExecute.GetParameters().Length != 2) return;
            methodToExecute.Invoke(this, new object[] {domainEvent, version});
        }
    }

    public abstract class ReadModel : Query
    {
        public abstract Type GetsCreatedOn { get; }
    }
}