using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microwave.ObjectPersistences
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