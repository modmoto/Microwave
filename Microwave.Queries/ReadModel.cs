using System;

namespace Microwave.Queries
{
    public abstract class ReadModel : Query
    {
        public abstract Type GetsCreatedOn { get; }
    }
}