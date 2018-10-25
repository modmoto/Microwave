namespace Application.Framework.Results
{
    public abstract class Result
    {
        public abstract void Check();

        public static Result ConcurrencyResult(long expectedVersion, long actualVersion)
        {
            return new ConcurrencyError(expectedVersion, actualVersion);
        }

        public static Result Ok()
        {
            return new Ok();
        }

        public bool Is<T>()
        {
            return typeof(T) == GetType();
        }
    }
}