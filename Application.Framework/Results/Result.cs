using System;

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

        public bool Is<T>() where T : Result
        {
            return typeof(T) == GetType();
        }
    }

    public abstract class Result<T>
    {
        public abstract T Value { get; }

        public static Result ConcurrencyResult(long expectedVersion, long actualVersion)
        {
            return new ConcurrencyError(expectedVersion, actualVersion);
        }

        public static Result<T> Ok(T value)
        {
            return new Ok<T>(value);
        }

        public bool Is<TCheck>() where TCheck : Result
        {
            return typeof(TCheck) == GetType();
        }

        public static Result<T> NotFound(string notFoundId)
        {
            return new NotFoundResult<T>(notFoundId);
        }
    }

    public class NotFoundResult<T> : Result<T>
    {
        public NotFoundResult(string notFoundId)
        {
            NotFoundId = notFoundId;
        }

        public override T Value => throw new NotFoundException(typeof(T), NotFoundId);

        public string NotFoundId { get; }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(Type type, string id) : base ($"Could not find entity {type.Name} with ID {id}")
        {
        }
    }
}