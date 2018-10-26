namespace Application.Framework.Results
{
    internal class Ok : Result
    {
        public override void Check()
        {
        }
    }

    internal class Ok<T> : Result<T>
    {
        internal Ok(T value)
        {
            Value = value;
        }

        public sealed override T Value { get; }
    }
}