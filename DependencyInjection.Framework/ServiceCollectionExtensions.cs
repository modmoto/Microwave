using System;
using System.Linq;
using System.Reflection;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using EventStore.ClientAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreFacadeDependencies(this IServiceCollection services,
            Assembly assembly, IEventStoreConnection connection)
        {
            connection.ConnectAsync().Wait();
            services.AddSingleton(connection);

            services.AddTransient<DomainEventConverter>();

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
                addSingletonForQuerry.Invoke(services, new[] {services, (object) querry});
                var addSingletonForSubscridedEvents =
                    addSingletonFunctionConcrete.MakeGenericMethod(subscribeEventTypeForQuerry.GetType());
                addSingletonForSubscridedEvents.Invoke(services, new[] {services, subscribeEventTypeForQuerry});
            }

            var addTransientWithInterfaceAndImplementation = typeof(ServiceCollectionServiceExtensions).GetMethods()
                .First(method =>
                    method.Name == "AddTransient" && method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 1);

            var addTransientWithClass = typeof(ServiceCollectionServiceExtensions).GetMethods().First(method =>
                method.Name == "AddTransient" && method.IsGenericMethod &&
                method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 1);

            var handlerTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IQuerryEventHandler)));
            foreach (var handlerType in handlerTypes)
            {
                var addSingleton =
                    addTransientWithInterfaceAndImplementation.MakeGenericMethod(typeof(IQuerryEventHandler), handlerType);
                addSingleton.Invoke(services, new object[] {services});

                var addSingleton2 = addTransientWithClass.MakeGenericMethod(handlerType);
                addSingleton2.Invoke(services, new object[] {services});
            }

            var reactivehandlerTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IReactiveEventHandler)));
            foreach (var handlerType in reactivehandlerTypes)
            {
                var addSingleton =
                    addTransientWithInterfaceAndImplementation.MakeGenericMethod(typeof(IReactiveEventHandler), handlerType);
                addSingleton.Invoke(services, new object[] {services});

                var addSingleton2 = addTransientWithClass.MakeGenericMethod(handlerType);
                addSingleton2.Invoke(services, new object[] {services});
            }

            return services;
        }

        public static IServiceCollection AddMyEventStoreDependencies(this IServiceCollection services,
            Assembly assembly)
        {
            services.AddTransient<IEventStoreFacade, MyEventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<DomainEventConverter>();
            services.AddDbContext<EventStoreContext>(option => option.UseSqlite("Data Source=Eventstore.db"));
            return services;
        }
    }
}