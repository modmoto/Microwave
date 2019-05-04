using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Application;
using Microwave.Discovery;
using Microwave.Discovery.Domain;
using Microwave.Discovery.Domain.Events;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.EventStores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;
using Microwave.WebApi.ApiFormatting;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
using Microwave.WebApi.Discovery;
using Microwave.WebApi.Filters;
using Microwave.WebApi.Monitoring;
using Microwave.WebApi.Querries;
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
                asyncEventDelegator.StartEventPolling();
                #pragma warning restore 4014
            });

            Task.Run(() =>
            {
                Task.Delay(10000).Wait();
                #pragma warning disable 4014
                asyncEventDelegator.StartDependencyDiscovery();
                #pragma warning restore 4014
            });

            return builder;
        }

        private static void AddEventAndReadModelSubscriptions(IServiceCollection services,
            IEnumerable<Assembly> readModelAndDomainEventAssemblies)
        {
            var iHandleAsyncEvents = new List<EventSchema>();
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

            var subscribedEventCollection = new EventsSubscribedByService(iHandleAsyncEvents, readModelSubscriptions);
            services.AddSingleton(subscribedEventCollection);
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
                    var assemblyName = AssemblyName.GetAssemblyName(path);
                    assemblies.Add(AppDomain.CurrentDomain.Load(assemblyName));
                }
                catch (FileNotFoundException)
                {
                }
            });

            services.AddMicrowaveMvcExtensions();

            services.AddTransient<IServiceDiscoveryRepository, DiscoveryRepository>();
            services.AddTransient<IDiscoveryHandler, DiscoveryHandler>();
            services.AddSingleton(new ServiceBaseAddressCollection());

            services.AddTransient<DomainEventController>();
            services.AddTransient<DiscoveryController>();
            services.AddTransient<IDiscoveryClientFactory, DiscoveryClientFactory>();
            services.AddTransient<IStatusRepository, StatusRepository>();
            services.AddTransient<MonitoringController>();

            services.AddTransient<JSonHack>();
            services.AddTransient<DomainEventWrapperListDeserializer>();

            services.AddTransient<IEventStore, EventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();

            services.AddTransient<JSonHack>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<MicrowaveDatabase>();
            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddTransient<IDomainEventClientFactory, DomainEventClientFactory>();
            services.AddSingleton(microwaveConfiguration);
            services.AddSingleton(microwaveConfiguration.ServiceLocations);
            services.AddSingleton(microwaveConfiguration.DatabaseConfiguration);

            AddEventAndReadModelSubscriptions(services, assemblies);
            AddPublishedEventCollection(services, assemblies, microwaveConfiguration);

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

        private static void AddPublishedEventCollection(IServiceCollection services,
            IEnumerable<Assembly> domainEventAssemblies, MicrowaveConfiguration microwaveConfiguration)
        {
            var publishedEventCollection = new PublishedEventsByServiceDto { ServiceName = microwaveConfiguration.ServiceName };
            foreach (var assembly in domainEventAssemblies)
            {
                var eventsForPublish = GetEventsForPublish(assembly);
                var notAddedYet = eventsForPublish.Where(e => publishedEventCollection.PublishedEvents.All(w => w.Name != e.Name));
                publishedEventCollection.PublishedEvents.AddRange(notAddedYet);
            }

            services.AddSingleton(publishedEventCollection);
        }

        private static IEnumerable<EventSchema> GetEventsForPublish(Assembly assembly)
        {
            var domainEvents = assembly.GetTypes().Where(e => e.GetInterfaces().Contains(typeof(IDomainEvent)));

            return domainEvents.Select(e =>
            {
                var propertyTypes = e.GetProperties().Select(p => new PropertyType(p.Name, p.PropertyType.Name));
                return new EventSchema(e.Name, propertyTypes);
            });
        }

        private static IEnumerable<EventSchema> GetEventsForSubscribe(Assembly assembly)
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
                    && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
                domainEvents.AddRange(domainEventTypes);
            }

            return domainEvents.Select(e =>
            {
                var propertyInfos = e.GetGenericArguments().First().GetProperties().ToList();
                var propertyTypes = propertyInfos.Select(p => new PropertyType(p.Name, p.PropertyType.Name));
                return new EventSchema(e.GetGenericArguments().First().Name, propertyTypes);
            });
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

                var propertyTypes = createdType.GetProperties().Select(p => new PropertyType(p.Name, p.PropertyType.Name));

                var readModelSubscription = new ReadModelSubscription(
                    readModel.Name,
                    new EventSchema(createdType.Name, propertyTypes));
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
                var eventName = domainEventType.Name;
                if (eventRegistration.ContainsKey(eventName))
                {
                    if (eventRegistration[eventName] != domainEventType)
                    {
                        throw new DuplicateDomainEventException(eventName);
                    }
                }
                eventRegistration.Add(eventName, domainEventType);
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

        private static IServiceCollection AddQuerryHandling(this IServiceCollection services, Assembly assembly)
        {
            var addTransient = AddTransient();

            var queryInterfaces = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndQuerry);
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(QueryEventHandler<,>);
            var iHandleType = typeof(IQueryEventHandler);

            foreach (var query in queryInterfaces)
            {
                var types = query.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>));
                foreach (var iHandleEvent in types)
                {
                    //feed
                    var domainEventType = iHandleEvent.GenericTypeArguments.Single();
                    var genericHandler = genericTypeOfHandler.MakeGenericType(query, domainEventType);
                    var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                    var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                    var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                    addTransientCall.Invoke(null, new object[] { services });

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

            var handleAsyncInterfaces = assembly.GetTypes().Where(ImplementsIhandleAsyncInterface);
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(AsyncEventHandler<>);
            var genericTypeOfHandlerInterface = typeof(IAsyncEventHandler);
            var handleAsyncType = typeof(IHandleAsync<>);

            foreach (var handleAsync in handleAsyncInterfaces)
            {
                var types = handleAsync.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
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

            foreach (var readModel in readModels)
            {
                var genericReadModelHandler = genericTypeOfHandler.MakeGenericType(readModel);
                //feed
                var genericHandler = genericReadModelHandler;
                var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                var addTransientCall = addTransient.MakeGenericMethod(feedInterface, feed);
                addTransientCall.Invoke(null, new object[] { services });

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