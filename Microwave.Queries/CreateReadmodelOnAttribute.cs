using System;
using System.Linq;
using Microwave.Domain;

namespace Microwave.Queries
{
    public class CreateReadmodelOnAttribute : Attribute
    {
        public Type CreationEvent { get; }

        public CreateReadmodelOnAttribute(Type creationEvent)
        {
            if (!creationEvent.GetInterfaces().Contains(typeof(IDomainEvent))) throw new ArgumentException($"Can not instantiate CreationEvent with a class that does not implement {nameof(IDomainEvent)}");
            CreationEvent = creationEvent;
        }
    }
}