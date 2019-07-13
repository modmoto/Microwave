using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Persistence.MongoDb;
using Microwave.UI;
using ReadService1;
using ServerConfig;

namespace WriteService2
{
    public class Startup
    {
        private MicrowaveConfiguration _microwaveConfiguration = new MicrowaveConfiguration
        {
            ServiceName = "WriteService2",
            ServiceLocations = ServiceConfiguration.ServiceAdresses,
            MicrowaveHttpClientCreator = new MyMicrowaveHttpClientCreator()
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new ApiKeyRequirement())
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMicrowaveUi();

            services.AddMicrowave(_microwaveConfiguration, new MongoDbPersistenceLayer
                { MicrowaveMongoDb = new MicrowaveMongoDb { DatabaseName = "TestWriteService2ReadDb" }} );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}