namespace Microwave.Domain.Validation
{
    public class TypelessDomainError : DomainError
    {
        internal TypelessDomainError(string errorType)
        {
            ErrorType = errorType;
        }

        public override string ErrorType { get; }
    }
}