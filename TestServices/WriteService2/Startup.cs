using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Domain.EventSourcing;
using Microwave.Persistence.InMemory;
using Microwave.Persistence.MongoDb;
using Microwave.UI;
using Microwave.WebApi;
using Microwave.WebApi.Queries;
using ReadService1;
using ServerConfig;

namespace WriteService2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new ApiKeyRequirement())
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMicrowaveUi();

            services.AddMicrowave(config =>
            {
                config.WithFeedType(typeof(EventFeed<>));
            });
            services.AddMicrowaveWebApi(config =>
            {
                config.WithServiceName("WriteService2");
                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);
                config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());
            });


            IEnumerable<IDomainEvent> events = new List<IDomainEvent>
            {
                new Event4("Event4")
            };
            services.AddMicrowavePersistenceLayerInMemory(o => o.WithEventSeeds(events));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}