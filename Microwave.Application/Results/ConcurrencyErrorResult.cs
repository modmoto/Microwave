using Microwave.Application.Exceptions;

namespace Microwave.Application.Results
{
    internal class ConcurrencyErrorResult : Result
    {
        public ConcurrencyErrorResult(long expectedVersion, long actualVersion) : base(new ConcurrencyError(expectedVersion, actualVersion))
        {
        }
    }

    internal class ConcurrencyError : ResultStatus
    {
        public long ExpectedVersion { get; }
        public long ActualVersion { get; }

        public ConcurrencyError(long expectedVersion, long actualVersion)
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }

        public override void Check()
        {
            throw new ConcurrencyViolatedException(ExpectedVersion, ActualVersion);
        }
    }
}