namespace Microwave.Application.Discovery
{
    public class PropertyType
    {
        public PropertyType(string name, string type, bool isPresentInRemote = false)
        {
            Name = name;
            Type = type;
            IsPresentInRemote = isPresentInRemote;
        }

        public string Name { get; }
        public string Type { get; }
        public bool IsPresentInRemote { get; }

        public override bool Equals(object obj)
        {
            if (obj is PropertyType schema)
            {
                return string.Equals(Name, schema.Name) && string.Equals(Type, schema.Type);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }
    }
}