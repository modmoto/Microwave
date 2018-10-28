using System;

namespace Application.Framework.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(long expectedVersion, long actualVersion) : base(
            $"Concurrency fraud detected, could not update database. ExpectedVersion: {expectedVersion}, ActualVersion: {actualVersion}")
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }

        public long ExpectedVersion { get; }
        public long ActualVersion { get; }
    }
}