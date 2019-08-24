using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microwave.WebApi.Discovery
{
    public class MicrowaveHttpContext
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static HttpContext Current => _httpContextAccessor.HttpContext;
        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";
        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _httpContextAccessor = contextAccessor;
        }
    }

    public static class HttpContextExtensions
    {
        public static IApplicationBuilder UseMicrowaveContext(this IApplicationBuilder app)
        {
            MicrowaveHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}