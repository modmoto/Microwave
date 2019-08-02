using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

[assembly: InternalsVisibleTo("Microwave")]
namespace Microwave.WebApi.Queries
{
    [Serializable]
    public class EventRegistration : Dictionary<string, Type>
    {
        public EventRegistration()
        {
        }

        protected EventRegistration(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}