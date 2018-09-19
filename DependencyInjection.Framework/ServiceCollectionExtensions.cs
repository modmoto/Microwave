using System;
using System.Linq;
using System.Reflection;
using Adapters.Framework.EventStores;
using Application.Framework;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuerryAndEventHandler(this IServiceCollection collection, Assembly assembly)
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

                var addSingletonForQuerry = addSingletonFunctionConcrete.MakeGenericMethod(querryType);
                addSingletonForQuerry.Invoke(collection, new[] {collection, (object) querry});
                var addSingletonForSubscridedEvents =
                    addSingletonFunctionConcrete.MakeGenericMethod(subscribeEventTypeForQuerry.GetType());
                addSingletonForSubscridedEvents.Invoke(collection, new[] {collection, subscribeEventTypeForQuerry});
            }

            var eventHandlers = assembly.GetTypes().Where(type => type.BaseType.IsGenericType
                                                                  && type.BaseType.GetGenericTypeDefinition() ==
                                                                  typeof(ReactiveEventHandler<>)).ToList();
            foreach (var eventHandlerType in eventHandlers)
            {
                var subscribedEventsType = typeof(SubscribedEventTypes<>);
                var genericTypeForEvents = subscribedEventsType.MakeGenericType(eventHandlerType);
                var subscribedEvents = Activator.CreateInstance(genericTypeForEvents);

                var addSingleton =
                    addSingletonFunctionConcrete.MakeGenericMethod(subscribedEvents.GetType());
                addSingleton.Invoke(collection,
                    new[] {collection, subscribedEvents});
            }

            var addTransientWithInterfaceAndImplementation = typeof(ServiceCollectionServiceExtensions).GetMethods()
                .First(method =>
                    method.Name == "AddTransient" && method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 1);

            var addTransientWithClass = typeof(ServiceCollectionServiceExtensions).GetMethods().First(method =>
                method.Name == "AddTransient" && method.IsGenericMethod &&
                method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 1);

            var handlerTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEventHandler)));
            foreach (var handlerType in handlerTypes)
            {
                var addSingleton =
                    addTransientWithInterfaceAndImplementation.MakeGenericMethod(typeof(IEventHandler), handlerType);
                addSingleton.Invoke(collection, new object[] {collection});

                var addSingleton2 = addTransientWithClass.MakeGenericMethod(handlerType);
                addSingleton2.Invoke(collection, new object[] {collection});
            }

            return collection;
        }

        public static void UseEventStoreSubscriptions(this IApplicationBuilder builder)
        {
            var builderApplicationServices = builder.ApplicationServices;
            var recallReferenceHolder =
                (QueryEventDelegator) builderApplicationServices.GetService(typeof(QueryEventDelegator));
            recallReferenceHolder.SubscribeToStreamsFrom();
        }
    }
}