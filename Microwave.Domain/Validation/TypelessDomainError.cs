namespace Microwave.Domain.Validation
{
    public class TypelessDomainError : DomainError
    {
        internal TypelessDomainError(string errorType, string detail)
        {
            ErrorType = errorType;
            Detail = detail;
        }

        public override string ErrorType { get; }
        public override string Detail { get; }
    }
}