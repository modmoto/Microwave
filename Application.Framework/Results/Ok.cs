namespace Application.Framework.Results
{
    public class Ok : Result
    {
        public override void Check()
        {
        }
    }

    public class Ok<T> : Result<T>
    {
        public Ok(T value)
        {
            Value = value;
        }

        public sealed override T Value { get; protected set; }
    }
}