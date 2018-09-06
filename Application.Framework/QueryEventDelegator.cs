using System;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryEventDelegator<T, TEvent> where T : Query where TEvent : DomainEvent
    {
        private readonly QueryHandler<T> _handler;
        private readonly IEventStoreQueue<TEvent> _eventQueuq;
        private Guid _lastProcessedEventId;

        public QueryEventDelegator(QueryHandler<T> handler, IEventStoreQueue<TEvent> eventQueuq)
        {
            _handler = handler;
            _eventQueuq = eventQueuq;
        }

        public async Task StartQueue()
        {
            while (_eventQueuq.HasNext())
            {
                var domainEvent = await _eventQueuq.GetNextFrom(_lastProcessedEventId);
                _handler.Handle(domainEvent);
                _lastProcessedEventId = domainEvent.Id;
            }
        }
    }

    public interface IEventStoreQueue<T> where T : DomainEvent
    {
        Task<T> GetNextFrom(Guid eventId);
        bool HasNext();
    }
}