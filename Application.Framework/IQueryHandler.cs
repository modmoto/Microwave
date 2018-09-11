using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQueryHandler
    {
        void Handle(DomainEvent domainEvent);
        IEnumerable<Type> SubscribedTypes { get; }
        Type HandledQuery { get; }
        void SetObject(Query snapShotQuerry);
    }

    public class SnapShot
    {
        public long Version { get; }
        public Query Querry { get; }

        public SnapShot(Query querry, long version)
        {
            Querry = querry;
            Version = version;
        }
    }
}