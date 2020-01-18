using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.InMemory;
using Microwave.UI;
using Microwave.WebApi;
using Microwave.WebApi.Queries;
using ServerConfig;

namespace WriteService1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMicrowaveUi();

            services.AddMicrowave(config =>
            {
                config.WithFeedType(typeof(EventFeed<>));
            });
            services.AddMicrowaveWebApi(config =>
            {
                config.WithServiceName("WriteService1");
                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);
                config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());
                config.SnapShots.Add(new SnapShot<EntityTest>(3));
            });


            IEnumerable<IDomainEvent> events = new List<IDomainEvent>
            {
                new Event2("ID1", "name1"),
                new Event2("ID2", "name2")
            };
            services.AddMicrowavePersistenceLayerInMemory( o=>
                o.WithEventSeeds(events));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
            app.RunMicrowaveServiceDiscovery();
        }
    }
}