using System;
using System.Linq;
using System.Reflection;
using Adapters.Framework.EventStores;
using Application.Framework;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllLoadedQuerries(this IServiceCollection collection, Assembly assembly,
            IEventStoreConnection conection, EventStoreConfig config)
        {
            var querries = assembly.GetTypes().Where(t => t.BaseType == typeof(Querry));
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), conection, config);
            var domainEvents = eventStore.GetEvents().Result.ToList();
            foreach (var querryType in querries)
            {
                var querry = (Querry) Activator.CreateInstance(querryType);
                foreach (var domainEvent in domainEvents)
                {
                    querry.Apply(domainEvent);
                }

                var addSingletonFunction = typeof(ServiceCollectionServiceExtensions).GetMethods().Where(method =>
                    method.Name == "AddSingleton" && method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 2).ToList()[1];


                var genericAddSingletonMethod = addSingletonFunction.MakeGenericMethod(querryType);
                genericAddSingletonMethod.Invoke(collection, new[] {collection, (object) querry});
            }

            return collection;
        }
    }
}