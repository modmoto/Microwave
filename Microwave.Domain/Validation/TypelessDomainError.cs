namespace Microwave.Domain.Validation
{
    public class TypelessDomainError : DomainError
    {
        public TypelessDomainError(string errorType)
        {
            ErrorType = errorType;
        }

        public override string ErrorType { get; }
    }
}