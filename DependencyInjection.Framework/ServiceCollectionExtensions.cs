using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Adapters.Framework.EventStores;
using Adapters.Framework.Queries;
using Adapters.Framework.Subscriptions;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyEventStoreDependencies(this IServiceCollection services,
            Assembly assembly, IConfiguration configuration)
        {
            services.AddTransient<IEventStoreFacade, EventStore>();
            services.AddTransient<IObjectConverter, ObjectConverter>();
            services.AddDbContext<EventStoreReadContext>(option =>
                option.UseSqlite("Data Source=EventStoreReadContext.db"));
            services.AddDbContext<EventStoreWriteContext>(option =>
                option.UseSqlite("Data Source=EventStoreWriteContext.db"));
            services.AddTransient<IEntityStreamRepository, EntityStreamRepository>();
            services.AddDbContext<SubscriptionContext>(option =>
                option.UseSqlite("Data Source=SubscriptionContext.db"));
            services.AddDbContext<QueryStorageContext>(option =>
                option.UseSqlite("Data Source=QueryStorageContext.db"));
            services.AddTransient<IEntityStreamRepository, EntityStreamRepository>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IQeryRepository, QueryRepository>();
            services.AddTransient<IOverallProjectionRepository, OverallProjectionRepository>();
            services.AddTransient<ITypeProjectionRepository, TypeProjectionRepository>();

            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<IProjectionHandler, ProjectionHandler>();
            services.AddTransient<ITypeProjectionHandler, TypeProjectionHandler>();

            //services.AddTransient<IEventDelegateHandler, EventDelegateHandler<SeasonCreatedEvent>>();
            //services.AddIEventDelegateHander(assembly);

            //services.AddTransient<IHandleAsync<SeasonCreatedEvent>, SeasonCreatedEventHandler>();
            services.AddIHandleAsync(assembly);

            services.AddSingleton(new EventLocationConfig(configuration));

            return services;
        }

        public static IServiceCollection AddIHandleAsync(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t)).ToList();
            var genericTypeOfHandler = typeof(IHandleAsync<>);

            foreach (var handler in handlerInterfaces)
            {
                var eventTypes = handler.GetInterfaces();
                foreach (var eventType in eventTypes)
                {
                    var domainEventType = eventType.GenericTypeArguments.Single();
                    var handleAsyncInterfaceWithDomainEventType = genericTypeOfHandler.MakeGenericType(domainEventType);

                    var callToAddTransient = addTransient.MakeGenericMethod(handleAsyncInterfaceWithDomainEventType, handler);
                    callToAddTransient.Invoke(null, new object[] { services });
                }
            }

            return services;
        }

        public static IServiceCollection AddIEventDelegateHander(this IServiceCollection services, Assembly assembly)
        {
            var methodInfos = typeof(ServiceCollectionServiceExtensions).GetMethods().ToList();
            var methodInfo = methodInfos.Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);

            var handleAsyndTypes = assembly.GetTypes().Where(t => ImplementsIEventDelegateHandlerInterface(t)).ToList();
            var genericTypeOfHandler = typeof(EventDelegateHandler<>);

            var domainEventTypes = handleAsyndTypes.Select(ha => ha.GenericTypeArguments.Single());
            foreach (var ihandleInterfaces in domainEventTypes)
            {
                var ihandleInterfacesGenericTypeArguments = ihandleInterfaces.GenericTypeArguments.Single();
                Type[] typeArgs = {ihandleInterfacesGenericTypeArguments};
                var makeGenericType = genericTypeOfHandler.MakeGenericType(typeArgs);
                var makeGenericMethod = methodInfo.MakeGenericMethod(typeof(IEventDelegateHandler), makeGenericType);
                makeGenericMethod.Invoke(services, new object[] { });
            }

            return services;
        }

        private static bool ImplementsIEventDelegateHandlerInterface(Type type)
        {
            return type.GetInterfaces().Contains(typeof(IEventDelegateHandler));
        }

        private static bool ImplementsIhandleAsyncInterface(Type myType)
        {
            return myType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
        }
    }
}