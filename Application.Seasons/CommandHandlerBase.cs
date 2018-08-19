using Application.Framework;

namespace Application.Seasons
{
    public class CommandHandlerBase
    {
        protected readonly IEventStore EventStore;

        public CommandHandlerBase(IEventStore eventStore)
        {
            EventStore = eventStore;
        }
    }
}