namespace Microwave.Domain.Validation
{
    public class DomainError
    {
        public DomainError(string description = null)
        {
            Description = description;
        }

        public string ErrorType => GetType().Name;
        public string Description { get; }
    }
}