namespace Microwave.Domain.Identities
{
    public class StringIdentity : Identity
    {
        private StringIdentity(string id)
        {
            Id = id;
        }

        public new static StringIdentity Create(string id)
        {
            return new StringIdentity(id);
        }
    }
}