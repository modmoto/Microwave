using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microwave.UI
{
    public static class MicrowaveUiExtensions
    {
        [ExcludeFromCodeCoverage]
        public static void AddMicrowaveUi(this IServiceCollection services)
        {
            services.ConfigureOptions(typeof(MicrowaveUiConfigureOptions));
        }

        [ExcludeFromCodeCoverage]
        public static IApplicationBuilder UseMicrowaveUi(this IApplicationBuilder builder)
        {
            builder.UseRouting();
            builder.UseEndpoints(endpoints => AddEnpoints(endpoints));
            builder.UseStaticFiles();
            return builder;
        }

        [ExcludeFromCodeCoverage]
        private static IEndpointRouteBuilder AddEnpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            return endpoints;
        }
    }
}