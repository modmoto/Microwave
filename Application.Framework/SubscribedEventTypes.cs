using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Application.Framework
{
    public abstract class SubscribedEventTypes<T> : List<Type>
    {
        public SubscribedEventTypes()
        {
            var methodInfos = typeof(T).GetMethods().Where(method => method.Name == nameof(Query.Apply) && method.GetParameters().Length == 1);
            foreach (var method in methodInfos)
            {
                var parameterInfo = method.GetParameters()[0];
                if (parameterInfo.ParameterType.BaseType == typeof(DomainEvent))
                    Add(parameterInfo.ParameterType);
            }
        }
    }
}