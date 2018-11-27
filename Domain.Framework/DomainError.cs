namespace Domain.Framework
{
    public class DomainError
    {
        public DomainError(string description)
        {
            Description = description;
        }

        public string ErrorType => GetType().Name;
        public string Description { get; }
    }
}