namespace Microwave.Domain.Validation
{
    public class TypelessDomainError : DomainErrorRenamed
    {
        public TypelessDomainError(string errorType)
        {
            ErrorType = errorType;
        }

        public override string ErrorType { get; }
    }
}