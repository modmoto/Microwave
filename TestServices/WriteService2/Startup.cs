using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Application;
using ServerConfig;

namespace WriteService2
{
    public class Startup
    {
        private MicrowaveConfiguration _microwaveConfiguration = new MicrowaveConfiguration
        {
            ServiceName = "WriteService2",
            ServiceLocations = ServiceConfiguration.ServiceAdresses,
            DatabaseConfiguration = new DatabaseConfiguration
            {
                DatabaseName = "TestWriteService2ReadDb"
            }
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMicrowave(_microwaveConfiguration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.RunMicrowaveQueries();
        }
    }
}