using System;
using System.Runtime.Serialization;

namespace Microwave.Application.Exceptions
{
    [Serializable]
    public class ConcurrencyViolatedException : Exception
    {
        protected ConcurrencyViolatedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        public ConcurrencyViolatedException(long expectedVersion, long actualVersion) : base(
            $"Concurrency fraud detected, could not update database. ExpectedVersion: {expectedVersion}, ActualVersion: {actualVersion}")
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }

        public long ExpectedVersion { get; }
        public long ActualVersion { get; }
    }
}