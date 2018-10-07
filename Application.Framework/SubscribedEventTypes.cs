using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Application.Framework
{
    public class SubscribedEventTypes<T> : List<string>
    {
        public SubscribedEventTypes()
        {
            var methodInfos = typeof(T).GetMethods().Where(method => (method.Name == nameof(Query.Apply) || method.Name == "Handle") && method.GetParameters().Length == 1);
            foreach (var method in methodInfos)
            {
                var parameterInfo = method.GetParameters()[0];
                if (parameterInfo.ParameterType.BaseType == typeof(DomainEvent))
                    Add(parameterInfo.ParameterType.Name);
            }
        }
    }
}