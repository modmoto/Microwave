using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Adapters.WebApi.Seasons;
using Application.Framework;
using Application.Seasons;
using Application.Seasons.Querries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineLeagueBackend
{
    public class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<SeasonController>();
            services.AddTransient<IEventStore, EventStore>();
            services.AddTransient<IObjectPersister<AllSeasonsQuery>, ObjectPersister<AllSeasonsQuery>>();
            services.AddTransient<IDomainEventPersister, DomainEventPersister>();

            services.AddTransient<SeasonCommandHandler>();

            services.AddTransient<SeasonQuerryHandler>();

            var querries = typeof(AllSeasonsQuery).Assembly.GetTypes().Where(t => t.BaseType == typeof(Querry));
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
                genericAddSingletonMethod.Invoke(services, new[] {services, allSeasonQuery});
            }
        }

        static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly,
            Type extendedType)
        {
            var query = from type in assembly.GetTypes()
                where type.IsSealed && !type.IsGenericType && !type.IsNested
                from method in type.GetMethods(BindingFlags.Static
                                               | BindingFlags.Public | BindingFlags.NonPublic)
                where method.IsDefined(typeof(ExtensionAttribute), false)
                where method.GetParameters()[0].ParameterType == extendedType
                select method;
            return query;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}