using Microsoft.Extensions.Configuration;

namespace Microwave.EventStores
{
    public class SnapShotConfig
    {
        private readonly IConfiguration _configuration;

        public SnapShotConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool DoesNeedSnapshot(string name, long oldVersion, long currentVersion)
        {
            var configurationSection = _configuration.GetSection("SnapShotTimes");
            if (!long.TryParse(configurationSection[name], out var snapShotTime)) return false;
            return  currentVersion - oldVersion > snapShotTime;
        }
    }
}