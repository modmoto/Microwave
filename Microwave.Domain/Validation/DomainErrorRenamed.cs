namespace Microwave.Domain.Validation
{
    public abstract class DomainErrorRenamed
    {
        public DomainErrorRenamed(string description = null)
        {
            Description = description;
        }

        public virtual string ErrorType => GetType().Name;
        public string Description { get; }
    }
}