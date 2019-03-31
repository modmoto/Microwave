using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Application;
using Microwave.Application.Discovery;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.WebApi;
using Microwave.WebApi.ApiFormatting;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
using Microwave.WebApi.Filters;
using MongoDB.Bson.Serialization;

namespace Microwave
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder RunMicrowaveQueries(this IApplicationBuilder builder)
        {
            var serviceScope = builder.ApplicationServices.CreateScope();
            var discoveryHandler = serviceScope.ServiceProvider.GetService<DiscoveryHandler>();

            Task.Run(() =>
            {
                Task.Delay(10000).Wait();
                discoveryHandler.DiscoverConsumingServices().Wait();
                var asyncEventDelegator = serviceScope.ServiceProvider.GetService<AsyncEventDelegator>();
                #pragma warning disable 4014
                asyncEventDelegator.Update();
                #pragma warning restore 4014
            });

            return builder;
        }

        private static void AddEventAndReadModelSubscriptions(IServiceCollection services,
            IEnumerable<Assembly> readModelAndDomainEventAssemblies)
        {
            var iHandleAsyncEvents = new List<string>();
            var modelAndDomainEventAssemblies = readModelAndDomainEventAssemblies.ToList();
            foreach (var assembly in modelAndDomainEventAssemblies)
            {
                var eventsForPublish = GetEventsForSubscribe(assembly);
                var notAddedYet = eventsForPublish.Where(e => !iHandleAsyncEvents.Contains(e));
                iHandleAsyncEvents.AddRange(notAddedYet);
            }

            var readModelSubscriptions = new List<ReadModelSubscription>();
            foreach (var assembly in modelAndDomainEventAssemblies)
            {
                var eventsForPublish = GetEventsForReadModelSubscribe(assembly);
                var notAddedYet = eventsForPublish.Where(e => !readModelSubscriptions.Contains(e));
                readModelSubscriptions.AddRange(notAddedYet);
            }

            services.AddSingleton(new SubscribedEventCollection(iHandleAsyncEvents, readModelSubscriptions));
        }

        public static IServiceCollection AddMicrowave(this IServiceCollection services)
        {
            services.AddMicrowave(new MicrowaveConfiguration());
            return services;
        }

        public static IServiceCollection AddMicrowave(this IServiceCollection services,
            MicrowaveConfiguration microwaveConfiguration)
        {
            var assemblies = new List<Assembly>();
            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories).ToList();
            referencedPaths.ForEach(path =>
            {
                try
                {
                    assemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                }
                catch (FileNotFoundException)
                {
                }
            });

            services.AddMicrowaveMvcExtensions();

            services.AddTransient<IServiceDiscoveryRepository, ServiceDiscoveryRepository>();
            services.AddTransient<DiscoveryHandler>();
            services.AddSingleton(new ServiceBaseAddressCollection());

            services.AddTransient<DomainEventController>();
            services.AddTransient<DiscoveryController>();
            services.AddTransient<IDiscoveryClientFactory, DiscoveryClientFactory>();
            services.AddTransient<MonitoringController>();
            services.AddSingleton<IEventLocation>(new EventLocation());

            services.AddTransient<JSonHack>();
            services.AddTransient<DomainEventWrapperListDeserializer>();

            services.AddTransient<IEventStore, EventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();
            services.AddTransient<EventDatabase>();

            services.AddTransient<JSonHack>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<ReadModelDatabase>();
            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddSingleton(microwaveConfiguration);
            services.AddSingleton(microwaveConfiguration.ServiceLocations);
            services.AddSingleton(microwaveConfiguration.ReadDatabase);
            services.AddSingleton(microwaveConfiguration.WriteDatabase);

            AddEventAndReadModelSubscriptions(services, assemblies);
            AddPublishedEventCollection(services, assemblies);

            foreach (var assembly in assemblies)
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            var eventRegistration = new EventRegistration();

            foreach (var assembly in assemblies)
            {
                services.AddQuerryHandling(assembly);
                services.AddAsyncEventHandling(assembly);
                services.AddReadmodelHandling(assembly);

                services.AddDomainEventRegistration(assembly, eventRegistration);
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            services.AddSingleton(eventRegistration);

            if (!BsonClassMap.IsClassMapRegistered(typeof(GuidIdentity))) BsonClassMap.RegisterClassMap<GuidIdentity>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(StringIdentity))) BsonClassMap.RegisterClassMap<StringIdentity>();

            return services;
        }

        private static void AddPublishedEventCollection(IServiceCollection services, IEnumerable<Assembly> domainEventAssemblies)
        {
            var publishedEventCollection = new PublishedEventCollection();
            foreach (var assembly in domainEventAssemblies)
            {
                var eventsForPublish = GetEventsForPublish(assembly);
                var notAddedYet = eventsForPublish.Where(e => !publishedEventCollection.Contains(e));
                publishedEventCollection.AddRange(notAddedYet);
            }

            services.AddSingleton(publishedEventCollection);
        }

        private static IEnumerable<string> GetEventsForPublish(Assembly assembly)
        {
            var entityTypes = assembly.GetTypes().Where(ev => ev.GetInterfaces().Contains(typeof(IApply)));
            var domainEvents = new List<Type>();
            foreach (var entityType in entityTypes)
            {
                var interfaces = entityType.GetInterfaces();
                var domainEventTypes = interfaces.Where(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Length == 1
                    && i.GetGenericArguments().First().GetInterfaces().Contains(typeof(IDomainEvent)));
                domainEvents.AddRange(domainEventTypes);
            }

            return domainEvents.Select(e => e.GetGenericArguments().First().Name);
        }

        private static IEnumerable<string> GetEventsForSubscribe(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var handleAsyncs = types.Where(ev => ev.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IHandleAsync<>)));
            var domainEvents = new List<Type>();

            foreach (var handler in handleAsyncs)
            {
                var interfaces = handler.GetInterfaces();
                var domainEventTypes = interfaces.Where(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Length == 1
                    && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>)
                    && i.GetGenericArguments().First().GetInterfaces().Contains(typeof(IDomainEvent)));
                domainEvents.AddRange(domainEventTypes);
            }

            return domainEvents.Select(e => e.GetGenericArguments().First().Name);
        }

        private static IEnumerable<ReadModelSubscription> GetEventsForReadModelSubscribe(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var readModels = types.Where(ev =>
                ev.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IHandle<>)) &&
                typeof(ReadModel).IsAssignableFrom(ev)).ToList();
            var subscriptions = new List<ReadModelSubscription>();

            foreach (var readModel in readModels)
            {
                var instance = Activator.CreateInstance(readModel);
                var propertyInfo = readModel.GetProperty(nameof(ReadModel.GetsCreatedOn));
                var createdType = propertyInfo?.GetValue(instance) as Type;
                if (createdType == null) throw new InvalidReadModelCreationTypeException(readModel.Name);

                var readModelSubscription = new ReadModelSubscription(readModel.Name, createdType.Name);
                subscriptions.Add(readModelSubscription);
            }

            return subscriptions;
        }

        private static IServiceCollection AddDomainEventRegistration(this IServiceCollection services,
            Assembly assembly, EventRegistration eventRegistration)
        {
            var domainEventTypes = assembly.GetTypes().Where(ev => ev.GetInterfaces().Contains(typeof(IDomainEvent)));
            foreach (var domainEventType in domainEventTypes)
            {
                eventRegistration.Add(domainEventType.Name, domainEventType);
            }
            return services;
        }

        private static IServiceCollection AddMicrowaveMvcExtensions(this IServiceCollection services)
        {
            services.AddMvcCore(config =>
            {
                config.Filters.Add(new DomainValidationFilter());
                config.Filters.Add(new NotFoundFilter());
                config.Filters.Add(new ConcurrencyViolatedFilter());

                config.OutputFormatters.Insert(0, new NewtonsoftOutputFormatter());
                config.InputFormatters.Insert(0, new NewtonsoftInputFormatter());

                config.ModelBinderProviders.Insert(0, new IdentityModelBinderProvider());
                config.ModelBinderProviders.Insert(0, new DateTimeOffsetBinderProvider());
            });
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

        private static IServiceCollection AddAsyncEventHandling(this IServiceCollection services, Assembly
        assembly)
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