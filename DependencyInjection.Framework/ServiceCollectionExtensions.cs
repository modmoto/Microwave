using System.Reflection;
using Adapters.Framework.EventStores;
using Adapters.Framework.Queries;
using Adapters.Framework.Subscriptions;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyEventStoreDependencies(this IServiceCollection services,
            Assembly assembly, IConfiguration configuration)
        {
            services.AddTransient<IEventStoreFacade, EventStore>();
            services.AddTransient<IEntityStreamRepository, EntityStreamRepository>();
            services.AddTransient<IObjectConverter, ObjectConverter>();
            services.AddDbContext<EventStoreWriteContext>(option => option.UseSqlite("Data Source=EventStoreReadContext.db"));
            services.AddDbContext<EventStoreReadContext>(option => option.UseSqlite("Data Source=EventStoreWriteContext.db"));
            services.AddDbContext<SubscriptionContext>(option => option.UseSqlite("Data Source=SubscriptionContext.db"));
            services.AddDbContext<QueryStorageContext>(option => option.UseSqlite("Data Source=QueryStorageContext.db"));
            services.AddTransient<IEntityStreamRepository, EntityStreamRepository>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IQeryRepository, QueryRepository>();
            services.AddTransient<IOverallProjectionRepository, OverallProjectionRepository>();
            services.AddTransient<ITypeProjectionRepository, TypeProjectionRepository>();

            services.AddTransient<AsyncEventDelegator>();
            services.AddTransient<IProjectionHandler, ProjectionHandler>();
            services.AddTransient<ITypeProjectionHandler, TypeProjectionHandler>();

            services.AddSingleton(new EventLocationConfig(configuration));

            return services;
        }
    }
}