using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.InMemory;
using Microwave.Persistence.MongoDb;
using Microwave.UI;
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
                config.WithServiceName("WriteService1");
                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);
                config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());
                config.SnapShots.Add(new SnapShot<EntityTest>(3));
            });


            services.AddMicrowavePersistenceLayerInMemory();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(o => o.MapRazorPages());
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}