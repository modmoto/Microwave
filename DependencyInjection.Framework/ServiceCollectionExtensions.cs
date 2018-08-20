using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllLoadedQuerries(this IServiceCollection collection, Assembly assembly)
        {
            var querries = assembly.GetTypes().Where(t => t.BaseType == typeof(Querry));
            foreach (var querryType in querries)
            {
                var persisterType = typeof(ObjectPersister<>);
                Type[] typeArgs = { querryType };
                var persister = persisterType.MakeGenericType(typeArgs);
                object o = Activator.CreateInstance(persister);
                var type = o.GetType();
                var methodInfo = type.GetMethod("GetAsync");
                var task = (Task) methodInfo.Invoke(o, new object [0]);

                task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");
                var allSeasonQuery = resultProperty.GetValue(task);

                var addSingletonFunction = typeof(ServiceCollectionServiceExtensions).GetMethods().Where(method =>
                    method.Name == "AddSingleton" && method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 2).ToList()[1];


                var genericAddSingletonMethod = addSingletonFunction.MakeGenericMethod(allSeasonQuery.GetType());
                genericAddSingletonMethod.Invoke(collection, new[] {collection, allSeasonQuery});
            }

            return collection;
        }
    }
}