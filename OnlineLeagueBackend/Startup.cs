using System;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Adapters.WebApi.Seasons;
using Application.Framework;
using Application.Seasons;
using Application.Seasons.Querries;
using DependencyInjection.Framework;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddTransient<IEventSourcingStrategy, EventSourcingApplyStrategy>();
            services.AddTransient<EventStoreConfig, RealEventStoreConfig>();
            services.AddTransient<IEventStoreFacade, EventStoreFacade>();
            var connection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            connection.ConnectAsync().Wait();
            services.AddSingleton(connection);
            services.AddTransient<DomainEventConverter>();

            services.AddTransient<SeasonCommandHandler>();

            services.AddAllEmptyQuerries(typeof(AllSeasonsQuery).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseEventStoreSubscriptions();
            app.UseMvc();
        }
    }
}