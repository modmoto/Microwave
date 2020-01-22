using System;

namespace Microwave.Logging
{
    public interface IMicrowaveLogger<T>
    {
        void LogInformation(string message);
        void LogWarning(Exception exception, string message);
    }
}