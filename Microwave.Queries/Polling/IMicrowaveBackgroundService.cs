using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Polling
{
    public interface IMicrowaveBackgroundService
    {
        Task RunAsync();
        DateTime NextRun { get; }
    }
}