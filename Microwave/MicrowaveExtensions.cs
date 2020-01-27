using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microwave.EventStores;
using Microwave.Logging;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;

namespace Microwave
{
    public static class MicrowaveExtensions
    {
        private static Type _genericTypeOfFeed;

        public static IApplicationBuilder RunMicrowaveQueries(this IApplicationBuilder builder)
        {
            var serviceScope = builder.ApplicationServices.CreateScope();
            var asyncEventDelegator = serviceScope.ServiceProvider.GetService<AsyncEventDelegator>();

            Task.Run(() =>
            {
                Task.Delay(10000).Wait();
                asyncEventDelegator.StartEventPolling();
            });

            return builder;
        }

        public static IServiceCollection AddMicrowave(
            this IServiceCollection services,
            Action<MicrowaveConfiguration> addConfiguration)
        {
            var microwaveConfiguration = new MicrowaveConfiguration();
            addConfiguration.Invoke(microwaveConfiguration);
            _genericTypeOfFeed = microwaveConfiguration.FeedType;

            var assemblies = GetAllAssemblies();

            services.AddTransient<IEventStore, EventStore>();

            services.AddTransient<AsyncEventDelegator>();
            services.AddSingleton(typeof(IMicrowaveLogger<>), typeof(MicrowaveLogger<>));
            services.AddSingleton(microwaveConfiguration.LogLevel);


            foreach (var assembly in assemblies)
            {
                services.AddQuerryHandling(assembly);
                services.AddAsyncEventHandling(assembly);
                services.AddReadmodelHandling(assembly);
            }

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
                    Console.WriteLine($"ADDED ASS {assemblyName} on {path}");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"did not find {path}");
                }
            });
            return assemblies;
        }

        private static IServiceCollection AddQuerryHandling(this IServiceCollection services, Assembly assembly)
        {
            var queryInterfaces = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndQuerry);
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
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
                    var feed = _genericTypeOfFeed.MakeGenericType(genericHandler);
                    var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                    services.AddTransient(feedInterface, feed);

                    //handler
                    services.AddTransient(iHandleType, genericHandler);

                    Console.WriteLine($"Added qhandler for {query.Name}");
                }
            }

            return services;
        }

        private static IServiceCollection AddAsyncEventHandling(this IServiceCollection services, Assembly
        assembly)
        {
            var handleAsyncInterfaces = assembly.GetTypes().Where(ImplementsIhandleAsyncInterface);
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
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
                    var feed = _genericTypeOfFeed.MakeGenericType(genericHandler);
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

                        Console.WriteLine($"Added async handlers for {domainEventType}");
                        return createdHandlerInstance;

                    });

                    //handleAsyncs
                    var handleAsyncTypeWithEvent = handleAsyncType.MakeGenericType(domainEventType);
                    services.AddTransient(handleAsyncTypeWithEvent, handleAsync);

                    Console.WriteLine($"Added other async type {handleAsync.Name}");
                }
            }

            return services;
        }

        private static IServiceCollection AddReadmodelHandling(this IServiceCollection services, Assembly assembly)
        {
            var readModels = assembly.GetTypes().Where(ImplementsIhandleInterfaceAndReadModel).ToList();
            var genericInterfaceTypeOfFeed = typeof(IEventFeed<>);
            var genericTypeOfHandler = typeof(ReadModelEventHandler<>);
            var interfaceReadModelHandler = typeof(IReadModelEventHandler);

            foreach (var readModel in readModels)
            {
                var genericReadModelHandler = genericTypeOfHandler.MakeGenericType(readModel);
                //feed
                var genericHandler = genericReadModelHandler;
                var feed = _genericTypeOfFeed.MakeGenericType(genericHandler);
                var feedInterface = genericInterfaceTypeOfFeed.MakeGenericType(genericHandler);
                services.AddTransient(feedInterface, feed);

                //handler
                services.AddTransient(interfaceReadModelHandler, genericReadModelHandler);

                Console.WriteLine($"Added rm handlers for {readModel.Name}");
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