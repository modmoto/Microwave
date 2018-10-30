using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Framework.Queries;
using Adapters.Framework.Subscriptions;
using Adapters.Framework.WebApi;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
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
            services.AddTransient<DomainEventController>();

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
            services.AddIEventDelegateHandler(assembly);

            //services.AddTransient<IHandleAsync<SeasonCreatedEvent>, SeasonCreatedEventHandler>();
            services.AddIHandleAsync(assembly);

            //Client
            services.AddEventClient(assembly);
            services.AddEventFeed(assembly);

            //QueryHandlers
            services.AddQueryHandler(assembly);


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

        public static IServiceCollection AddIEventDelegateHandler(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t));
            var genericTypeOfDelegateHandler = typeof(EventDelegateHandler<>);

            var interfacesWithDomainEventImplementation = handlerInterfaces.SelectMany(i => i.GetInterfaces().Where(i2 => i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].BaseType == typeof(DomainEvent))).ToList();
            var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

            foreach (var domainEventType in domainEventTypes)
            {
                var delegateHandler = genericTypeOfDelegateHandler.MakeGenericType(domainEventType);
                var addTransientCall = addTransient.MakeGenericMethod(typeof(IEventDelegateHandler), delegateHandler);
                addTransientCall.Invoke(null, new object[] { services });
            }

            return services;
        }


        public static IServiceCollection AddEventClient(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1);

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t));
            var genericTypeOfClient = typeof(DomainEventClient<>);

            var interfacesWithDomainEventImplementation = handlerInterfaces.SelectMany(i => i.GetInterfaces().Where(i2 => i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].BaseType == typeof(DomainEvent))).ToList();
            var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

            foreach (var domainEventType in domainEventTypes)
            {
                var delegateHandler = genericTypeOfClient.MakeGenericType(domainEventType);
                var addTransientCall = addTransient.MakeGenericMethod(delegateHandler);
                addTransientCall.Invoke(null, new object[] { services });
            }

            return services;
        }

        public static IServiceCollection AddEventFeed(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t));
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);

            var interfacesWithDomainEventImplementation = handlerInterfaces.SelectMany(i => i.GetInterfaces().Where(i2 => i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].BaseType == typeof(DomainEvent))).ToList();
            var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

            foreach (var domainEventType in domainEventTypes)
            {
                var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(domainEventType);
                var feed = genericTypeOfFeed.MakeGenericType(domainEventType);
                var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                addTransientCall.Invoke(null, new object[] { services });
            }

            return services;
        }

        public static IServiceCollection AddQueryHandler(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);

            var handlerInterface = typeof(IQueryEventHandler);
            var genericTypeOfHandler = typeof(QueryEventHandler<,>);

            var allQuerries = assembly.GetTypes().Where(t => t.BaseType == typeof(Query));

            foreach (var querry in allQuerries)
            {
                var interfacesWithDomainEventImplementation = querry.GetInterfaces().Where(i2 => i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].BaseType == typeof(DomainEvent)).ToList();
                var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

                foreach (var domainEventType in domainEventTypes)
                {
                    var handler = genericTypeOfHandler.MakeGenericType(querry, domainEventType);
                    var addTransientCall = addTransient.MakeGenericMethod(handlerInterface, handler);
                    addTransientCall.Invoke(null, new object[] { services });
                }
            }

            return services;
        }

        private static bool ImplementsIhandleAsyncInterface(Type myType)
        {
            return myType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
        }

        private static bool ImplementsIhandleInterface(Type myType)
        {
            return myType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>));
        }
    }
}