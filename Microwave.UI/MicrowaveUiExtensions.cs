using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microwave.UI
{
    public static class MicrowaveUiExtensions
    {
        public static void AddMicrowaveUi(this IServiceCollection services)
        {
            services.ConfigureOptions(typeof(MicrowaveUiConfigureOptions));
            services.AddSingleton(new MicrowaveUiConfiguration());
        }

        public static IApplicationBuilder UseMicrowaveUi(this IApplicationBuilder builder)
        {
            builder.UseStaticFiles();
            return builder;
        }
    }
}