using System;
using System.Threading.Tasks;

namespace Microwave.Subscriptions.Ports
{
    public interface IRemoteVersionRepository
    {
        Task<DateTimeOffset> GetCurrentVersion(Subscription subscription);
        Task SaveRemoteVersionAsync(RemoteVersion version);
        Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion);
    }
}