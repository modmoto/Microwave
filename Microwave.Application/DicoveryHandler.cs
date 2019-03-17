namespace Microwave.Application
{
    public class DicoveryHandler
    {
        private readonly ServiceLocations _serviceLocations;
        private readonly SubscribedEventCollection _subscribedEventCollection;

        public DicoveryHandler(ServiceLocations serviceLocations, SubscribedEventCollection subscribedEventCollection)
        {
            _serviceLocations = serviceLocations;
            _subscribedEventCollection = subscribedEventCollection;
        }
    }
}