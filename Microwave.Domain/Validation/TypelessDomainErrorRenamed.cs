namespace Microwave.Domain.Validation
{
    public class TypelessDomainErrorRenamed : DomainErrorRenamed
    {
        public TypelessDomainErrorRenamed(string errorType)
        {
            ErrorType = errorType;
        }

        public override string ErrorType { get; }
    }
}