using Adapters.Framework.EventStores;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OnlineLeagueBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            var recallReferenceHolder = (QueryEventDelegator) webHost.Services.GetService(typeof(QueryEventDelegator));
            recallReferenceHolder.SubscribeToStreams().Wait();
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}