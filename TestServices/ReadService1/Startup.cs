using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microwave;
using Microwave.Logging;
using Microwave.Persistence.InMemory;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.UI;
using Microwave.WebApi;
using Microwave.WebApi.Queries;
using ServerConfig;

namespace ReadService1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddMicrowaveUi();
            services.AddMicrowave(config =>
            {
                config.WithFeedType(typeof(EventFeed<>))
                    .WithLogLevel(MicrowaveLogLevel.Info);

                config.PollingIntervals.Add(new PollingInterval<Handler2>("0 0 1 * *"));
                config.PollingIntervals.Add(new PollingInterval<ReadModel1>(25));
                config.PollingIntervals.Add(new PollingInterval<Querry1>(5));
            });

            services.AddMicrowaveWebApi(config =>
            {
                config.WithHttpClientFactory(new MyMicrowaveHttpClientFactory());

                config.ServiceLocations.AddRange(ServiceConfiguration.ServiceAdresses);

                config.WithServiceName("ReadService1");
            });

            services.AddMicrowavePersistenceLayerInMemory();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
            app.UseMicrowaveUi();
        }
    }
}