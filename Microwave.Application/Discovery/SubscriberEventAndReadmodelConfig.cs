using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public class SubscriberEventAndReadmodelConfig
    {
        public Uri ServiceBaseAddress { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }
        public string ServiceName { get; }

        public SubscriberEventAndReadmodelConfig(
            Uri serviceBaseAddress,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels,
            string serviceName = null)
        {
            ServiceBaseAddress = serviceBaseAddress;
            SubscribedEvents = subscribedEvents;
            ReadModels = readModels;
            ServiceName = serviceName ?? serviceBaseAddress.ToString();
        }
    }

    public class EventSchema
    {
        public EventSchema(string name, IEnumerable<PropertyType> properties = null)
        {
            Name = name;
            Properties = new List<PropertyType>
            {
                new PropertyType("Name", typeof(string).Name),
                new PropertyType("Alter", typeof(int).Name)
            };
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

    public class PropertyType
    {
        public PropertyType(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; }
    }
}