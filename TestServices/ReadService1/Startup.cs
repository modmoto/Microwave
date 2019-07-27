using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Persistence.MongoDb;
using Microwave.Queries.Polling;
using Microwave.UI;
using ServerConfig;

namespace ReadService1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMicrowaveUi();
            services.AddMicrowave(config =>
                {
                    config.PollingIntervals.Add(new PollingInterval<Handler2>(10));
                    config.PollingIntervals.Add(new PollingInterval<ReadModel1>(25));
                    config.PollingIntervals.Add(new PollingInterval<Querry1>(5));

                    config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());

                    config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);

                    config.WithServiceName("ReadService1");
                });

            services.AddMicrowavePersistenceLayerMongoDb(p =>
            {
                p.WithDatabaseName("TestReadService1");
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