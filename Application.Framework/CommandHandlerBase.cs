namespace Application.Framework
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