using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.WebApi;
using Microwave.WebApi.Filters;
using Microwave.WebApi.IdentityFormatting;
using MongoDB.Bson.Serialization;

namespace Microwave
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder RunMicrowaveQueries(this IApplicationBuilder builder)
        {
            var serviceScope = builder.ApplicationServices.CreateScope();
            var asyncEventDelegator = serviceScope.ServiceProvider.GetService<AsyncEventDelegator>();
            Task.Run(() =>
            {
                Task.Delay(10000).Wait();
                #pragma warning disable 4014
                asyncEventDelegator.Update();
                #pragma warning restore 4014
            });

            return builder;
        }

        public static IServiceCollection AddMicrowaveReadModels(this IServiceCollection services,
            Assembly readModelAssembly, IConfiguration configuration)
        {
            services.AddTransient(option =>
            {
                return new ReadModelDatabase(configuration);
            });

            //WebApi
            services.AddMvcCore(config =>
            {
                config.Filters.Add(new NotFoundFilter());

                config.OutputFormatters.Insert(0, new NewtonsoftOutputFormatter());
                config.InputFormatters.Insert(0, new NewtonsoftInputFormatter());
                config.ModelBinderProviders.Insert(0, new IdentityModelBinderProvider());
                config.ModelBinderProviders.Insert(0, new DateTimeOffsetBinderProvider());
            });

            services.AddTransient<DomainEventWrapperListDeserializer>();
            services.AddTransient<JSonHack>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddSingleton<IEventLocationConfig>(new EventLocationConfig(configuration));

            services.AddQuerryHandling(readModelAssembly);
            services.AddEventDelegateHandling(readModelAssembly);
            services.AddReadmodelHandling(readModelAssembly);

            services.AddDomainEventRegistration(readModelAssembly);

            if (!BsonClassMap.IsClassMapRegistered(typeof(GuidIdentity))) BsonClassMap.RegisterClassMap<GuidIdentity>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(StringIdentity))) BsonClassMap.RegisterClassMap<StringIdentity>();

            return services;
        }

        public static IServiceCollection AddMicrowave(this IServiceCollection services,
            Assembly domainEventAssembly, IConfiguration configuration)
        {
            services.AddTransient(option =>
            {
                return new EventDatabase(configuration);
            });

            services.AddTransient<DomainEventController>();
            services.AddTransient<JSonHack>();
            services.AddTransient<DomainEventWrapperListDeserializer>();

            services.AddTransient<IEventStore, EventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();

            //WebApi
            services.AddMvcCore(config =>
            {
                config.Filters.Add(new DomainValidationFilter());
                config.Filters.Add(new NotFoundFilter());
                config.Filters.Add(new ConcurrencyViolatedFilter());

                config.OutputFormatters.Insert(0, new NewtonsoftOutputFormatter());
                config.InputFormatters.Insert(0, new NewtonsoftInputFormatter());
                config.ModelBinderProviders.Insert(0, new IdentityModelBinderProvider());
            });

            services.RegisterBsonClassMaps(domainEventAssembly);

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

        private static IServiceCollection RegisterBsonClassMaps(this IServiceCollection services, Assembly assembly)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(GuidIdentity))) BsonClassMap.RegisterClassMap<GuidIdentity>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(StringIdentity))) BsonClassMap.RegisterClassMap<StringIdentity>();

            var registerClassMapMethod = typeof(BsonClassMap).GetMethods().Single(m => m.Name == nameof(BsonClassMap
            .RegisterClassMap) && m.IsGenericMethod && m.GetParameters().Length == 0);
            var domainEventTypes = assembly.GetTypes().Where(ev => ev.GetInterfaces().Contains(typeof(IDomainEvent)));
            foreach (var domainEventType in domainEventTypes)
            {
                var makeGenericMethod = registerClassMapMethod.MakeGenericMethod(domainEventType);
                makeGenericMethod.Invoke(null, null);
            }
            return services;
        }

        private static bool IsDomainEvent(Type i2)
        {
            return i2.GenericTypeArguments.Length == 1 && i2.GenericTypeArguments[0].GetInterfaces().Contains(typeof(IDomainEvent));
        }

        private static IServiceCollection AddQuerryHandling(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var queryInterfaces = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndQuerry).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(QueryEventHandler<,>);
            var clientType = typeof(DomainEventClient<>);
            var iHandleType = typeof(IQueryEventHandler);

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

                    //handler
                    var callToAddTransient = addTransient.MakeGenericMethod(iHandleType, genericHandler);
                    callToAddTransient.Invoke(null, new object[] { services });
                }
            }

            return services;
        }

        private static IServiceCollection AddEventDelegateHandling(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var handleAsyncInterfaces = assembly.GetTypes().Where(ImplementsIhandleAsyncInterface).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(AsyncEventHandler<>);
            var genericTypeOfHandlerInterface = typeof(IAsyncEventHandler);
            var clientType = typeof(DomainEventClient<>);
            var handleAsyncType = typeof(IHandleAsync<>);

            foreach (var handleAsync in handleAsyncInterfaces)
            {
                var types = handleAsync.GetInterfaces().Where(IsDomainEvent).ToList();
                bool added = false;
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

                    //handler
                    if (!added)
                    {
                        var callToAddTransient = addTransient.MakeGenericMethod(genericTypeOfHandlerInterface, genericHandler);
                        callToAddTransient.Invoke(null, new object[] { services });
                        added = true;
                    }

                    //handleAsyncs
                    var handleAsyncTypeWithEvent = handleAsyncType.MakeGenericType(domainEventType);
                    var callToAddTransientHandleAsyncs = addTransient.MakeGenericMethod(handleAsyncTypeWithEvent, handleAsync);
                    callToAddTransientHandleAsyncs.Invoke(null, new object[] { services });
                }
            }

            return services;
        }

        private static IServiceCollection AddReadmodelHandling(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();
            var addTransientSingle = AddTransientSingle();

            var readModels = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndReadModel).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(ReadModelHandler<>);
            var interfaceReadModelHandler = typeof(IReadModelHandler);
            var clientType = typeof(DomainEventClient<>);

            foreach (var readModel in readModels)
            {
                var genericReadModelHandler = genericTypeOfHandler.MakeGenericType(readModel);
                //feed
                var genericHandler = genericReadModelHandler;
                var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                addTransientCall.Invoke(null, new object[] { services });

                //client
                var genericClient = clientType.MakeGenericType(genericHandler);
                var addTransientCallClient = addTransientSingle.MakeGenericMethod(genericClient);
                addTransientCallClient.Invoke(null, new object[] {services});

                //handler
                var callToAddTransient = addTransient.MakeGenericMethod(interfaceReadModelHandler, genericReadModelHandler);
                callToAddTransient.Invoke(null, new object[] { services });
            }

            return services;
        }

        private static bool ImplementsIhandleAsyncInterface(Type myType)
        {
            return myType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
        }

        private static bool ImplementsIhandleInterfaceAndQuerry(Type myType)
        {
            return myType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>)) && myType.BaseType == typeof(Query);
        }

        private static bool ImplementsIhandleInterfaceAndReadModel(Type myType)
        {
            return myType.GetInterfaces()
                       .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>)) && myType.BaseType == typeof(ReadModel);
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