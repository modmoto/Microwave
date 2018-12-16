using Microwave.Application.Exceptions;

namespace Microwave.Application.Results
{
    public class ConcurrencyErrorResult : Result
    {
        public ConcurrencyErrorResult(long expectedVersion, long actualVersion) : base(new ConcurrencyError(expectedVersion, actualVersion))
        {
        }
    }

    public class ConcurrencyError : ResultStatus
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