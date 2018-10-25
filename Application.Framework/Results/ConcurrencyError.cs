using Application.Framework.Exceptions;

namespace Application.Framework.Results
{
    public class ConcurrencyError : Result
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
            throw new ConcurrencyException(ExpectedVersion, ActualVersion);
        }
    }
}