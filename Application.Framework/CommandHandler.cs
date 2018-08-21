namespace Application.Framework
{
    public class CommandHandler
    {
        protected readonly IEventStore EventStore;

        public CommandHandler(IEventStore eventStore)
        {
            EventStore = eventStore;
        }
    }
}