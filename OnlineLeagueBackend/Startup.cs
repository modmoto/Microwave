using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Adapters.WebApi.Seasons;
using Application.Framework;
using Application.Seasons;
using Application.Seasons.Querries;
using DependencyInjection.Framework;
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
            services.AddSingleton<IEventStore, EventStore>();
            services.AddTransient<IObjectPersister<AllSeasonsQuery>, ObjectPersister<AllSeasonsQuery>>();
            services.AddTransient<IDomainEventPersister, DomainEventPersister>();

            services.AddTransient<SeasonCommandHandler>();

            services.AddTransient<AllSeasonsQuerryHandler>();

            services.AddAllLoadedQuerries(typeof(AllSeasonsQuery).Assembly);
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