using System;

namespace Microwave.Application.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(Type type, string id) : base ($"Could not find entity {type.Name} with ID {id}")
        {
        }
    }
}