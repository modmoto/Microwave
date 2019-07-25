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
        private MicrowaveConfiguration _microwaveConfiguration = new MicrowaveConfiguration
        {
            ServiceName = "ReadService1",
            ServiceLocations = ServiceConfiguration.ServiceAdresses,
            MicrowaveHttpClientCreator = new MyMicrowaveHttpClientCreator(),
            UpdateEveryConfigurations = new List<IPollingInterval>
            {
                new PollingInterval<Handler2>(10),
                new PollingInterval<ReadModel1>(25),
                new PollingInterval<Querry1>(5)
            }
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMicrowaveUi();
            services.AddMicrowave(_microwaveConfiguration, new MongoDbPersistenceLayer
                { MicrowaveMongoDb = new MicrowaveMongoDb { DatabaseName = "TestReadService1" }});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}