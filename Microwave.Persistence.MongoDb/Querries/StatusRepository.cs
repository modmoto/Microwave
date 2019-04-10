using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.Domain;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class StatusRepository : IStatusRepository
    {
        private readonly IMongoDatabase _database;
        private const string StatusDbName = "MicrowaveStatusCollection";
        private Guid EventLocationId => new Guid("78448B83-1BA9-44DF-935B-78EC9B3D1FA4");
        private Guid ServiceMapId => new Guid("7DF86BEB-92FF-401A-9415-106AD83DED90");

        public StatusRepository(MicrowaveDatabase database)
        {
            _database = database.Database;
        }
        public async Task SaveEventLocation(EventLocation eventLocation)
        {
            var eventLocationDbo = new EventLocationDbo
            {
                Id = EventLocationId,
                Services = eventLocation.Services,
                UnresolvedEventSubscriptions = eventLocation.UnresolvedEventSubscriptions,
                UnresolvedReadModeSubscriptions = eventLocation.UnresolvedReadModeSubscriptions
            };

            await InsertOrUpdate(eventLocationDbo);
        }

        private async Task InsertOrUpdate<T>(T eventLocationDbo) where T : IIdentifiable
        {
            var mongoCollection = _database.GetCollection<T>(StatusDbName);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<T>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<T, bool>>) (e => e.Id == eventLocationDbo.Id),
                eventLocationDbo,
                findOneAndReplaceOptions);
        }

        public async Task<IEventLocation> GetEventLocation()
        {
            var mongoCollection = _database.GetCollection<EventLocationDbo>(StatusDbName);
            var location = await mongoCollection.FindSync(e => e.Id == EventLocationId).SingleOrDefaultAsync();
            return location == null ? null : new EventLocation(location.Services, location.UnresolvedEventSubscriptions, location.UnresolvedReadModeSubscriptions);
        }

        public async Task<ServiceMap> GetServiceMap()
        {
            var mongoCollection = _database.GetCollection<ServiceMapDbo>(StatusDbName);
            var location = await mongoCollection.FindSync(e => e.Id == ServiceMapId).SingleOrDefaultAsync();
            return location == null ? null : new ServiceMap(location.Services.Select(s =>
                ServiceNodeWithDependentServices.Reachable(s.ServiceEndPoint, s.Services)));
        }

        public async Task SaveServiceMap(ServiceMap map)
        {
            var serviceMapDbo = new ServiceMapDbo
            {
                Id = ServiceMapId,
                Services = map.AllServices
            };


            await InsertOrUpdate(serviceMapDbo);
        }
    }

    internal interface IIdentifiable
    {
        Guid Id { get; }
    }

    public class EventLocationDbo : IIdentifiable
    {
        public IEnumerable<ServiceNode> Services { get; set; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; set; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; set; }
        public Guid Id { get; set; }
    }

    public class ServiceMapDbo : IIdentifiable
    {
        public ServiceEndPoint ServiceEndPoint { get; set; }
        public IEnumerable<ServiceNodeWithDependentServices> Services { get; set; }
        public Guid Id { get; set; }
    }
}