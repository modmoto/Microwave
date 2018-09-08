namespace Application.Framework
{
    public class CommandHandler
    {
        protected readonly IEventStoreFacade EventStoreFacade;

        public CommandHandler(IEventStoreFacade eventStoreFacade)
        {
            EventStoreFacade = eventStoreFacade;
        }
    }
}