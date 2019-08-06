using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.MongoDb;
using Microwave.UI;
using ServerConfig;

namespace WriteService1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMicrowaveUi();

            services.AddMicrowave(config =>
            {
                config.WithServiceName("WriteService1");
                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);
                config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());
                config.SnapShots.Add(new SnapShot<EntityTest>(3));
            });

            services.AddMicrowavePersistenceLayerMongoDb(p =>
            {
                p.WithDatabaseName("TestWriteService1ReadDb");
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}