using System;
using System.Threading.Tasks;
using Microwave.Queries;
using WriteService2;

namespace ReadService1
{
    public class Handler1 : IHandleAsync<DomainEvent1_Service2>
    {
        public Task HandleAsync(DomainEvent1_Service2 domainEvent)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ReadModel1 : ReadModel, IHandle<DomainEvent1_Service2>, IHandle<DomainEvent2_Service2>
    {
        public override Type GetsCreatedOn => typeof(DomainEvent1_Service2);
        public void Handle(DomainEvent1_Service2 domainEvent)
        {
            throw new NotImplementedException();
        }

        public void Handle(DomainEvent2_Service2 domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}