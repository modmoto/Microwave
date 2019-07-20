using System.Linq;

namespace Microwave.Queries
{
    public class Query
    {
        public void Handle(ISubscribedDomainEvent domainEvent, long version)
        {
            var type = domainEvent.GetType();
            var currentEntityType = GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Handle));
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            var parameterInfos = methodToExecute?.GetParameters().ToList();
            if (methodToExecute != null && parameterInfos.Count == 1)
            {
                methodToExecute.Invoke(this, new object[] {domainEvent});
                return;
            }

            if (methodToExecute != null && parameterInfos.Count == 2
                                        && parameterInfos[1].ParameterType == typeof(long))
            {
                methodToExecute.Invoke(this, new object[] {domainEvent, version});
            }
        }
    }
}