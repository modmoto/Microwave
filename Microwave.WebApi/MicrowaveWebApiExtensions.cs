using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores.SnapShots;
using Microwave.Queries;
using Microwave.WebApi.Discovery;
using Microwave.WebApi.Filters;
using Microwave.WebApi.Queries;

namespace Microwave.WebApi
{
    public static class MicrowaveWebApiExtensions
    {
        public static IServiceCollection AddMicrowaveWebApi(
            this IServiceCollection services)
        {
            services.AddMicrowaveWebApi(config => { });
            return services;
        }
        public static IServiceCollection AddMicrowaveWebApi(
            this IServiceCollection services,
            Action<MicrowaveWebApiConfiguration> addConfiguration)
        {
            var microwaveConfiguration = new MicrowaveWebApiConfiguration();
            addConfiguration.Invoke(microwaveConfiguration);

            services.AddMicrowaveMvcExtensions();

            services.AddSingleton<ISnapShotConfig>(new SnapShotConfig(microwaveConfiguration.SnapShots));
            services.AddSingleton(microwaveConfiguration.PollingIntervals);

            services.AddSingleton(microwaveConfiguration);
            services.AddSingleton(microwaveConfiguration.ServiceLocations);
            services.AddSingleton(microwaveConfiguration.MicrowaveHttpClientFactory);
            services.AddSingleton(new DiscoveryConfiguration { ServiceName = microwaveConfiguration.ServiceName });


            services.AddTransient<IServiceDiscoveryRepository, DiscoveryRepository>();

            services.AddTransient<DomainEventController>();
            services.AddTransient<DiscoveryController>();
            services.AddTransient<IDiscoveryClientFactory, DiscoveryClientFactory>();

            services.AddTransient<IDomainEventFactory, DomainEventFactory>();
            services.AddTransient<IDomainEventClientFactory, DomainEventClientFactory>();

            var eventRegistration = new EventRegistration();

            var assemblies = GetAllAssemblies();

            AddPublishedEventCollection(services, assemblies, microwaveConfiguration);

            foreach (var assembly in assemblies)
            {
                services.AddDomainEventRegistration(assembly, eventRegistration);
            }

            services.AddSingleton(eventRegistration);

            return services;
        }

        private static IServiceCollection AddMicrowaveMvcExtensions(this IServiceCollection services)
        {
            services.AddMvc(config =>
                {
                    config.Filters.Add(new DomainValidationFilter());
                    config.Filters.Add(new NotFoundFilter());
                    config.Filters.Add(new ConcurrencyViolatedFilter());
                })
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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

        private static void AddPublishedEventCollection(IServiceCollection services,
            IEnumerable<Assembly> domainEventAssemblies, MicrowaveWebApiConfiguration microwaveWebApiConfiguration)
        {
            var publishedEvents = new List<EventSchema>();
            foreach (var assembly in domainEventAssemblies)
            {
                var eventsForPublish = GetEventsForPublish(assembly);
                var notAddedYet = eventsForPublish.Where(e => publishedEvents.All(w => w.Name != e.Name));
                publishedEvents.AddRange(notAddedYet);
            }

            var publishedEventCollection = EventsPublishedByService.Reachable(
                new ServiceEndPoint(null, microwaveWebApiConfiguration.ServiceName),
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
    }
}