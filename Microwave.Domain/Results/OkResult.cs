namespace Microwave.Domain.Results
{
    public class OkResult : Result
    {
        public OkResult() : base(new Ok())
        {
        }
    }

    public class Ok<T> : Result<T>
    {
        public Ok(T value) : base(new Ok())
        {
            Value = value;
        }
    }

    public class Ok : ResultStatus
    {
        public override void Check()
        {
        }
    }
}