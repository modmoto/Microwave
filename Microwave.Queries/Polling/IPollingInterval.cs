using System;

namespace Microwave.Queries.Polling
{
    public interface IPollingInterval
    {
        Type AsyncCallType { get; }
        DateTime Next { get; }
    }
}