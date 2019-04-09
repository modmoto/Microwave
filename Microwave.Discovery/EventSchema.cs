using System.Collections.Generic;

namespace Microwave.Discovery
{
    public class EventSchema
    {
        public EventSchema(string name, IEnumerable<PropertyType> properties = null)
        {
            Name = name;
            Properties = properties ?? new List<PropertyType>();
        }

        public string Name { get; }
        public IEnumerable<PropertyType> Properties { get; }

        public override bool Equals(object obj)
        {
            if (obj is EventSchema schema)
            {
                return string.Equals(Name, schema.Name);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}