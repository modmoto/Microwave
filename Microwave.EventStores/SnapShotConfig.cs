using Microsoft.Extensions.Configuration;

namespace Microwave.EventStores
{
    public interface ISnapShotConfig
    {
        bool DoesNeedSnapshot(string name, long oldVersion, long currentVersion);
    }

    public class SnapShotConfig : ISnapShotConfig
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

            if (currentVersion % snapShotTime == 0 && currentVersion != oldVersion) return true;
            if (currentVersion - oldVersion >= snapShotTime) return true;
            if (currentVersion == oldVersion) return false;
            return (currentVersion - oldVersion + currentVersion) % snapShotTime == 0;
        }
    }
}