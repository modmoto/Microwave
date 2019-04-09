namespace Microwave.Discovery.Domain.Events
{
    public class ReadModelSubscription
    {
        public ReadModelSubscription(string readModelName, EventSchema getsCreatedOn)
        {
            ReadModelName = readModelName;
            GetsCreatedOn = getsCreatedOn;
        }

        public string ReadModelName { get; }
        public EventSchema GetsCreatedOn { get; }

        public override bool Equals(object obj)
        {
            if (obj is ReadModelSubscription rms)
            {
                return rms.ReadModelName == ReadModelName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ReadModelName != null ? ReadModelName.GetHashCode() : 0;
        }
    }
}