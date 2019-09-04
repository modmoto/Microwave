using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Persistence.InMemory;
using Microwave.UI;
using ServerConfig;

namespace WriteService2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMicrowaveUi();

            services.AddMicrowave(config =>
            {
                config.WithServiceName("WriteService2");
                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);
            });

//            services.AddMicrowavePersistenceLayerMongoDb(p =>
//            {
//                p.WithDatabaseName("TestWriteService2ReadDb");
//            });

            services.AddMicrowavePersistenceLayerInMemory();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}