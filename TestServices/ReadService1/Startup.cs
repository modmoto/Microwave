using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Domain;
using RazorUIClassLib;
using ServerConfig;

namespace ReadService1
{
    public class Startup
    {
        private MicrowaveConfiguration _microwaveConfiguration = new MicrowaveConfiguration
        {
            ServiceName = "ReadService1",
            ServiceLocations = ServiceConfiguration.ServiceAdresses,
            DatabaseConfiguration = new DatabaseConfiguration
            {
                DatabaseName = "TestReadService1"
            }
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMicrowaveUi();
            services.AddMicrowave(_microwaveConfiguration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseMicrowaveUi();
            app.RunMicrowaveQueries();
        }
    }
}