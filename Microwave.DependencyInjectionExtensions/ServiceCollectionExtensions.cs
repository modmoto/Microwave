using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Application;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using Microwave.Queries;
using Microwave.WebApi;
using Microwave.WebApi.Filters;

namespace Microwave.DependencyInjectionExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder EnsureMicrowaveDatabaseCreated(this IApplicationBuilder builder)
        {
            using (var serviceScope = builder.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var eventStoreContext = serviceScope.ServiceProvider.GetService<EventStoreContext>();
                var queryStorageContext = serviceScope.ServiceProvider.GetService<QueryStorageContext>();

                eventStoreContext?.Database.EnsureCreated();
                queryStorageContext?.Database.EnsureCreated();
            }

            return builder;
        }

        public static IServiceCollection AddMicrowaveQuerries(this IServiceCollection services,
            Assembly assembly, IConfiguration configuration)
        {
            services.AddDbContext<QueryStorageContext>(option =>
                option.UseSqlite("Data Source=QueryStorageContext.db"));

            //WebApi
            services.AddMvcCore(config =>
            {
                config.Filters.Add(new NotFoundFilter());
            });

            services.AddTransient<DomainEventWrapperListDeserializer>();
            services.AddTransient<JSonHack>();
            services.AddTransient<IObjectConverter, ObjectConverter>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IQeryRepository, QueryRepository>();
            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddSingleton<IEventLocationConfig>(new EventLocationConfig(configuration));

            //Handler
            services.AddIEventDelegateHandler(assembly);
            services.AddIHandleAsync(assembly);

            //Client
            services.AddEventClient(assembly);
            services.AddQuerryEventFeed(assembly);
            services.AddEventDelegateHandlerFeed(assembly);

            //QueryHandlers
            services.AddQueryHandler(assembly);
            services.AddReadmodelHandlerAndClients(assembly);

            services.AddDomainEventRegistration(assembly);

            return services;
        }

        public static IServiceCollection AddMicrowave(this IServiceCollection services)
        {
            services.AddTransient<DomainEventController>();
            services.AddTransient<JSonHack>();
            services.AddTransient<DomainEventDeserializer>();
            services.AddTransient<DomainEventWrapperListDeserializer>();

            services.AddTransient<IEventStore, EventStore>();
            services.AddTransient<IObjectConverter, ObjectConverter>();
            services.AddDbContext<EventStoreContext>(option =>
                option.UseSqlite("Data Source=EventStoreContext.db"));
            services.AddTransient<IEventRepository, EventRepository>();

            //WebApi
            services.AddMvcCore(config =>
            {
                config.Filters.Add(new DomainValidationFilter());
                config.Filters.Add(new NotFoundFilter());
                config.Filters.Add(new ConcurrencyViolatedFilter());
            });

            return services;
        }

        private static IServiceCollection AddDomainEventRegistration(this IServiceCollection services, Assembly assembly)
        {
            var domainEventTypes = assembly.GetTypes().Where(ev => ev.GetInterfaces().Contains(typeof(IDomainEvent)));
            var eventRegistration = new EventRegistration();
            foreach (var domainEventType in domainEventTypes)
            {
                eventRegistration.Add(domainEventType.Name, domainEventType);
            }
            services.AddSingleton(eventRegistration);
            return services;
        }

        private static IServiceCollection AddIHandleAsync(this IServiceCollection services, Assembly assembly)
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

        private static IServiceCollection AddIEventDelegateHandler(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t));
            var genericTypeOfDelegateHandler = typeof(EventDelegateHandler<>);

            var interfacesWithDomainEventImplementation = handlerInterfaces.SelectMany(i => i.GetInterfaces().Where(IsDomainEvent)).ToList();
            var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

            foreach (var domainEventType in domainEventTypes)
            {
                var delegateHandler = genericTypeOfDelegateHandler.MakeGenericType(domainEventType);
                var addTransientCall = addTransient.MakeGenericMethod(typeof(IEventDelegateHandler), delegateHandler);
                addTransientCall.Invoke(null, new object[] { services });
            }

            return services;
        }

        private static bool IsDomainEvent(Type i2)
        {
            return i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].GetInterfaces().Contains(typeof(IDomainEvent));
        }


        private static IServiceCollection AddEventClient(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1);

            var handlerInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleAsyncInterface(t));
            var handlerAsyncInterfaces = assembly.GetTypes().Where(t => ImplementsIhandleInterface(t));
            var allHandlerTypes = handlerInterfaces.ToList();
            allHandlerTypes.AddRange(handlerAsyncInterfaces.ToList());
            var genericTypeOfClient = typeof(DomainEventClient<>);

            var interfacesWithDomainEventImplementation = allHandlerTypes.SelectMany(i => i.GetInterfaces().Where(IsDomainEvent)).ToList();
            var domainEventTypes = interfacesWithDomainEventImplementation.Select(e => e.GenericTypeArguments.Single()).Distinct();

            foreach (var domainEventType in domainEventTypes)
            {
                var delegateHandler = genericTypeOfClient.MakeGenericType(domainEventType);
                var addTransientCall = addTransient.MakeGenericMethod(delegateHandler);
                addTransientCall.Invoke(null, new object[] { services });
            }

            return services;
        }

        private static IServiceCollection AddQuerryEventFeed(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var queryInterfaces = assembly.GetTypes().Where(ImplementsIhandleInterface).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(QueryEventHandler<,>);
            var clientType = typeof(NewDomainEventClient<>);

            foreach (var query in queryInterfaces)
            {
                var types = query.GetInterfaces().Where(IsDomainEvent).ToList();
                foreach (var iHandleEvent in types)
                {
                    //feed
                    var domainEventType = iHandleEvent.GenericTypeArguments.Single();
                    var genericHandler = genericTypeOfHandler.MakeGenericType(query, domainEventType);
                    var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                    var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                    var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                    addTransientCall.Invoke(null, new object[] { services });

                    //client
                    var genericClient = clientType.MakeGenericType(genericHandler);
                    var addTransientCallClient = addTransientSingle.MakeGenericMethod(genericClient);
                    addTransientCallClient.Invoke(null, new object[] {services});
                }
            }

            return services;
        }

        private static IServiceCollection AddEventDelegateHandlerFeed(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var handleAsyncInterfaces = assembly.GetTypes().Where(ImplementsIhandleAsyncInterface).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(EventDelegateHandler<>);
            var clientType = typeof(NewDomainEventClient<>);

            foreach (var handleAsync in handleAsyncInterfaces)
            {
                var types = handleAsync.GetInterfaces().Where(IsDomainEvent).ToList();
                foreach (var iHandleEvent in types)
                {
                    //feed
                    var domainEventType = iHandleEvent.GenericTypeArguments.Single();
                    var genericHandler = genericTypeOfHandler.MakeGenericType(domainEventType);
                    var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                    var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                    var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                    addTransientCall.Invoke(null, new object[] { services });

                    //client
                    var genericClient = clientType.MakeGenericType(genericHandler);
                    var addTransientCallClient = addTransientSingle.MakeGenericMethod(genericClient);
                    addTransientCallClient.Invoke(null, new object[] {services});
                }
            }

            return services;
        }

        private static IServiceCollection AddQueryHandler(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();

            var handlerInterface = typeof(IQueryEventHandler);
            var genericTypeOfHandler = typeof(QueryEventHandler<,>);

            var allQuerries = assembly.GetTypes().Where(t => t.BaseType == typeof(Query));

            foreach (var querry in allQuerries)
            {
                var interfacesWithDomainEventImplementation = querry.GetInterfaces().Where(IsDomainEvent).ToList();
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

        private static IServiceCollection AddReadmodelHandlerAndClients(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var handlerInterface = typeof(IReadModelHandler);
            var overallInterface = typeof(IEventFeed<>);
            var feedType = typeof(EventFeed<>);

            var genericTypeOfHandler = typeof(ReadModelHandler<>);
            var clientType = typeof(DomainOverallEventClient<>);

            var allReadModels = assembly.GetTypes().Where(t => t.BaseType == typeof(ReadModel));

            foreach (var readModel in allReadModels)
            {
                var handler = genericTypeOfHandler.MakeGenericType(readModel);
                var clientTypeGeneric = clientType.MakeGenericType(readModel);

                var readModelFeedType = feedType.MakeGenericType(handler);
                var overallInterfaceGeneric = overallInterface.MakeGenericType(handler);

                var addTransientCall = addTransient.MakeGenericMethod(handlerInterface, handler);
                var addTransientCallFeed = addTransient.MakeGenericMethod(overallInterfaceGeneric, readModelFeedType);
                var addClientTypeCall = addTransientSingle.MakeGenericMethod(clientTypeGeneric);

                addTransientCall.Invoke(null, new object[] { services });
                addTransientCallFeed.Invoke(null, new object[] { services });
                addClientTypeCall.Invoke(null, new object[] { services });
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

        private static MethodInfo AddTransientSingle()
        {
            return typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1);
        }

        private static MethodInfo AddTransient()
        {
            return typeof(ServiceCollectionServiceExtensions).GetMethods().Single(m =>
                m.Name == "AddTransient" && m.GetGenericArguments().Length == 2 &&
                m.GetParameters().Length == 1);
        }
    }
}