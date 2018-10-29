using System.Threading.Tasks;
using Application.Framework;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineLeagueBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            using (var serviceScope = webHost.Services.CreateScope())
            {
                var asyncEventDelegator = serviceScope.ServiceProvider.GetService<AsyncEventDelegator>();
                Task.Run(() => asyncEventDelegator.Update());
                webHost.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}