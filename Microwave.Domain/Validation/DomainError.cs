namespace Microwave.Domain.Validation
{
    public abstract class DomainError
    {
        public DomainError(string description = null)
        {
            Description = description;
        }

        public virtual string ErrorType => GetType().Name;
        public string Description { get; }
    }
}