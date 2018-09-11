using System;
using System.Linq;
using System.Reflection;
using Adapters.Framework.EventStores;
using Application.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllEmptyQuerries(this IServiceCollection collection, Assembly assembly)
        {
            collection.AddSingleton<QueryEventDelegator>();

            var querries = assembly.GetTypes().Where(t => t.BaseType == typeof(Query));

            var addSingletonFunctionConcrete = typeof(ServiceCollectionServiceExtensions).GetMethods().Where(method =>
                method.Name == "AddSingleton" && method.IsGenericMethod &&
                method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 2).ToList()[1];

            foreach (var querryType in querries)
            {
                var querry = (Query) Activator.CreateInstance(querryType);

                var subscribeEventTypeForQuerryType = typeof(SubscribedEventTypes<>);
                var genericType = subscribeEventTypeForQuerryType.MakeGenericType(querryType);
                var subscribeEventTypeForQuerry = Activator.CreateInstance(genericType);

                var genericAddSingletonMethod = addSingletonFunctionConcrete.MakeGenericMethod(querryType);
                genericAddSingletonMethod.Invoke(collection, new[] {collection, (object) querry});
                var genericAddSingletonMethod2 =
                    addSingletonFunctionConcrete.MakeGenericMethod(subscribeEventTypeForQuerry.GetType());
                genericAddSingletonMethod2.Invoke(collection, new[] {collection, subscribeEventTypeForQuerry});
            }

            var addSingletonFunctionTypeBase = typeof(ServiceCollectionServiceExtensions).GetMethods().First(method =>
                method.Name == "AddTransient" && method.IsGenericMethod &&
                method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 1);

            var addSingletonFunctionTypeBase2 = typeof(ServiceCollectionServiceExtensions).GetMethods().First(method =>
                method.Name == "AddTransient" && method.IsGenericMethod &&
                method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 1);

            var handlerTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IQueryHandler)));
            foreach (var handlerType in handlerTypes)
            {
                var addSingleton = addSingletonFunctionTypeBase.MakeGenericMethod(typeof(IQueryHandler), handlerType);
                addSingleton.Invoke(collection, new object[] {collection});

                var addSingleton2 = addSingletonFunctionTypeBase2.MakeGenericMethod(handlerType);
                addSingleton2.Invoke(collection, new object[] {collection});
            }

            return collection;
        }
    }
}