using System;

namespace Microwave.Logging
{
    public interface IMicrowaveLogger<T>
    {
        void LogTrace(string message);
        void LogWarning(Exception exception, string message);
    }
}