using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Discovery.Subscriptions;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.EventStores.SnapShots;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;
using Microwave.WebApi.ApiFormatting;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
using Microwave.WebApi.Discovery;
using Microwave.WebApi.Filters;
using Microwave.WebApi.Queries;

namespace Microwave
{
    public static class MicrowaveExtensions
    {
        public static IApplicationBuilder RunMicrowaveQueries(this IApplicationBuilder builder)
        {
            var serviceScope = builder.ApplicationServices.CreateScope();
            var url = builder.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.Single();
            var microwaveHttpContext = serviceScope.ServiceProvider.GetService<MicrowaveHttpContext>();
            microwaveHttpContext.Configure(new Uri(url));

            var asyncEventDelegator = serviceScope.ServiceProvider.GetService<AsyncEventDelegator>();

            Task.Run(() =>
            {
                Task.Delay(10000).Wait();
                asyncEventDelegator.StartEventPolling();
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
                var eventsForPublish = GetEventsForIHandleAsyncs(assembly);
                var eventsForPublish2 = GetEventsForQuerys(assembly);
                var notAddedYet = eventsForPublish.Where(e => !iHandleAsyncEvents.Contains(e));
                iHandleAsyncEvents.AddRange(notAddedYet);

                var notAddedYet2 = eventsForPublish2.Where(e => !iHandleAsyncEvents.Contains(e));
                iHandleAsyncEvents.AddRange(notAddedYet2);
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

        public static IServiceCollection AddMicrowave(
            this IServiceCollection services)
        {
            services.AddMicrowave(config => { });
            return services;
        }

        public static IServiceCollection AddMicrowave(
            this IServiceCollection services,
            Action<MicrowaveConfiguration> addConfiguration)
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            addConfiguration.Invoke(microwaveConfiguration);

            var assemblies = GetAllAssemblies();

            services.AddMicrowaveMvcExtensions();

            services.AddSingleton<ISnapShotConfig>(new SnapShotConfig(microwaveConfiguration.SnapShots));
            services.AddSingleton(microwaveConfiguration.PollingIntervals);

            services.AddTransient<IServiceDiscoveryRepository, DiscoveryRepository>();
            services.AddTransient<IDiscoveryHandler, DiscoveryHandler>();
            services.AddSingleton(new ServiceBaseAddressCollection());

            services.AddTransient<DomainEventController>();
            services.AddTransient<DiscoveryController>();
            services.AddTransient<IDiscoveryClientFactory, DiscoveryClientFactory>();
            services.AddTransient<IRemoteSubscriptionRepository, RemoteSubscriptionRepository>();
            services.AddSingleton<MicrowaveHttpContext>();

            services.AddTransient<IEventStore, EventStore>();

            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddTransient<IDomainEventClientFactory, DomainEventClientFactory>();

            services.AddSingleton(microwaveConfiguration);
            services.AddSingleton(microwaveConfiguration.ServiceLocations);
            services.AddSingleton(microwaveConfiguration.MicrowaveHttpClientFactory);
            services.AddSingleton(new DiscoveryConfiguration { ServiceName = microwaveConfiguration.ServiceName });

            AddEventAndReadModelSubscriptions(services, assemblies);
            AddPublishedEventCollection(services, assemblies, microwaveConfiguration);

            var eventRegistration = new EventRegistration();

            foreach (var assembly in assemblies)
            {
                services.AddQuerryHandling(assembly);
                services.AddAsyncEventHandling(assembly);
                services.AddReadmodelHandling(assembly);

                services.AddDomainEventRegistration(assembly, eventRegistration);
            }

            services.AddSingleton(eventRegistration);

            return services;
        }

        private static List<Assembly> GetAllAssemblies()
        {
            var assemblies = new List<Assembly>();
            var referencedPaths = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories).ToList();
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
            return assemblies;
        }

        private static void AddPublishedEventCollection(IServiceCollection services,
            IEnumerable<Assembly> domainEventAssemblies, MicrowaveConfiguration microwaveConfiguration)
        {
            var publishedEvents = new List<EventSchema>();
            foreach (var assembly in domainEventAssemblies)
            {
                var eventsForPublish = GetEventsForPublish(assembly);
                var notAddedYet = eventsForPublish.Where(e => publishedEvents.All(w => w.Name != e.Name));
                publishedEvents.AddRange(notAddedYet);
            }

            var publishedEventCollection = EventsPublishedByService.Reachable(
                new ServiceEndPoint(null, microwaveConfiguration.ServiceName),
                publishedEvents);

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

        private static IEnumerable<EventSchema> GetEventsForIHandleAsyncs(Assembly assembly)
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

        private static IEnumerable<EventSchema> GetEventsForQuerys(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var handleAsyncs = types.Where(ev => typeof(Query).IsAssignableFrom(ev) && ev.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IHandle<>)));
            var domainEvents = new List<Type>();

            foreach (var handler in handleAsyncs)
            {
                var interfaces = handler.GetInterfaces();
                var domainEventTypes = interfaces.Where(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Length == 1
                    && i.GetGenericTypeDefinition() == typeof(IHandle<>));
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
                typeof(ReadModelBase).IsAssignableFrom(ev)).ToList();
            var subscriptions = new List<ReadModelSubscription>();

            foreach (var readModel in readModels)
            {
                var instance = Activator.CreateInstance(readModel);
                var model = instance as ReadModelBase;
                var createdType = model?.GetsCreatedOn;
                if (createdType == null) throw new InvalidReadModelCreationTypeException(readModel.Name);

                var propertyTypes = createdType.GetProperties().Select(p => new PropertyType(p.Name, p.PropertyType.Name));

                var readModelSubscription = new ReadModelSubscription(
                    readModel.Name,
                    new EventSchema(createdType.Name, propertyTypes));
                subscriptions.Add(readModelSubscription);
            }

            return subscriptions;
        }

        public static IServiceCollection AddDomainEventRegistration(
            this IServiceCollection services,
            Assembly assembly,
            EventRegistration eventRegistration)
        {
            var remoteEvents =
                assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISubscribedDomainEvent)));

            foreach (var domainEventType in remoteEvents)
            {
                var eventName = domainEventType.Name;
                if (eventRegistration.ContainsKey(eventName))
                {
                    if (eventRegistration[eventName] != domainEventType)
                    {
                        throw new DuplicateDomainEventException(domainEventType);
                    }
                }
                else
                {
                    eventRegistration.Add(eventName, domainEventType);
                }
            }
            return services;
        }

        private static IServiceCollection AddMicrowaveMvcExtensions(this IServiceCollection services)
        {
            services.AddMvc(config =>
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
                    services.AddTransient(feedInterface, feed);

                    //handler
                    services.AddTransient(iHandleType, genericHandler);
                }
            }

            return services;
        }

        private static IServiceCollection AddAsyncEventHandling(this IServiceCollection services, Assembly
        assembly)
        {
            var handleAsyncInterfaces = assembly.GetTypes().Where(ImplementsIhandleAsyncInterface);
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(AsyncEventHandler<>);
            var genericTypeOfHandlerInterface = typeof(IAsyncEventHandler);
            var handleAsyncType = typeof(IHandleAsync<>);

            foreach (var handleAsync in handleAsyncInterfaces)
            {
                var types = handleAsync.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleAsync<>));
                services.AddTransient(handleAsync);

                foreach (var iHandleEvent in types)
                {
                    //feed
                    var domainEventType = iHandleEvent.GenericTypeArguments.Single();
                    var genericHandler = genericTypeOfHandler.MakeGenericType(domainEventType);
                    var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                    var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                    services.AddTransient(feedInterface, feed);

                    //handler
                    services.AddTransient(genericTypeOfHandlerInterface, s =>
                    {
                        var versionRepo = s.GetRequiredService<IVersionRepository>();
                        var feedInstance = s.GetRequiredService(feedInterface);
                        var handleAsyncInstance = s.GetRequiredService(handleAsync);
                        var constructorInfo = genericHandler.GetConstructors().Single();
                        var createdHandlerInstance = constructorInfo.Invoke(new [] { versionRepo, feedInstance, handleAsyncInstance });
                        return createdHandlerInstance;
                    });

                    //handleAsyncs
                    var handleAsyncTypeWithEvent = handleAsyncType.MakeGenericType(domainEventType);
                    services.AddTransient(handleAsyncTypeWithEvent, handleAsync);
                }
            }

            return services;
        }

        private static IServiceCollection AddReadmodelHandling(this IServiceCollection services, Assembly assembly)
        {
            var readModels = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndReadModel).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfFeed = typeof(EventFeed<>);
            var genericTypeOfHandler = typeof(ReadModelEventHandler<>);
            var interfaceReadModelHandler = typeof(IReadModelEventHandler);

            foreach (var readModel in readModels)
            {
                var genericReadModelHandler = genericTypeOfHandler.MakeGenericType(readModel);
                //feed
                var genericHandler = genericReadModelHandler;
                var feed = genericTypeOfFeed.MakeGenericType(genericHandler);
                var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                services.AddTransient(feedInterface, feed);

                //handler
                services.AddTransient(interfaceReadModelHandler, genericReadModelHandler);
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
                       .Any(i => i.IsGenericType
                                 && i.GetGenericTypeDefinition() == typeof(IHandle<>))
                                 && typeof(ReadModelBase).IsAssignableFrom(myType);
        }
    }
}