using System;
using System.Linq;
using System.Reflection;
using Application.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllLoadedQuerries(this IServiceCollection collection, Assembly assembly, IEventStoreFacade eventStoreFacade)
        {
            var querries = assembly.GetTypes().Where(t => t.BaseType == typeof(Query));
            var domainEvents = eventStoreFacade.GetEvents().Result.ToList();
            foreach (var querryType in querries)
            {
                var querry = (Query) Activator.CreateInstance(querryType);
                foreach (var domainEvent in domainEvents)
                {
                    querry.Apply(domainEvent);
                }

                var addSingletonFunction = typeof(ServiceCollectionServiceExtensions).GetMethods().Where(method =>
                    method.Name == "AddSingleton" && method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 2).ToList()[1];


                var subscribeEventTypeForQuerryType = typeof(SubscribedEventTypes<>);
                var genericType = subscribeEventTypeForQuerryType.MakeGenericType(querryType);
                var subscribeEventTypeForQuerry = Activator.CreateInstance(genericType);

                var genericAddSingletonMethod = addSingletonFunction.MakeGenericMethod(querryType);
                genericAddSingletonMethod.Invoke(collection, new[] {collection, (object) querry});
                var genericAddSingletonMethod2 = addSingletonFunction.MakeGenericMethod(subscribeEventTypeForQuerry.GetType());
                genericAddSingletonMethod2.Invoke(collection, new[] {collection, subscribeEventTypeForQuerry});
            }

            return collection;
        }
    }
}