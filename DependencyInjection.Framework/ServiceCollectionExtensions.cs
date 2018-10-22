using System.Reflection;
using Adapters.Framework.EventStores;
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
            services.AddTransient<IEventStoreFacade, Adapters.Framework.EventStores.EventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IDomainEventConverter, DomainEventConverter>();
            services.AddDbContext<EventStoreContext>(option => option.UseSqlite("Data Source=Eventstore.db"));
            services.AddDbContext<SubscriptionContext>(option => option.UseSqlite("Data Source=SubscriptionContext.db"));
            services.AddDbContext<QueryContext>(option => option.UseSqlite("Data Source=QueryContext.db"));
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<AsyncEventDelegator>();
            services.AddSingleton(new EventLocationConfig(configuration));

            return services;
        }
    }
}