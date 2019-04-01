using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Application;
using ServerConfig;

namespace WriteService1
{
    public class Startup
    {
        private MicrowaveConfiguration _microwaveConfiguration = new MicrowaveConfiguration
        {
            ServiceName = "WriteService1",
            ServiceLocations = ServiceConfiguration.ServiceAdresses,
            WriteDatabase = new WriteDatabaseConfig
            {
                DatabaseName = "TestWriteService1"
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