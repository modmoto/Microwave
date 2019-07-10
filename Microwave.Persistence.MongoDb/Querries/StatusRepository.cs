using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class StatusRepository : IStatusRepository
    {
        private readonly EventLocationCache _cache;
        private readonly IMongoDatabase _database;
        private const string StatusDbName = "MicrowaveStatusCollection";

        public StatusRepository(MicrowaveMongoDb mongoDb, EventLocationCache cache)
        {
            _cache = cache;
            _database = mongoDb.Database;
        }

        public async Task SaveEventLocation(EventLocation eventLocation)
        {
            var eventLocationDbo = new EventLocationDbo
            {
                Services = eventLocation.Services,
                UnresolvedEventSubscriptions = eventLocation.UnresolvedEventSubscriptions,
                UnresolvedReadModeSubscriptions = eventLocation.UnresolvedReadModeSubscriptions
            };
            _cache.Update(eventLocation);

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

        public async Task<EventLocation> GetEventLocation()
        {
            if (_cache.HasValue) return _cache.GetValue();
            var mongoCollection = _database.GetCollection<EventLocationDbo>(StatusDbName);
            var location = await mongoCollection.FindSync(e => e.Id == nameof(EventLocation)).SingleOrDefaultAsync();
            return location == null ? EventLocation.Default() : new EventLocation(location.Services, location
            .UnresolvedEventSubscriptions, location.UnresolvedReadModeSubscriptions);
        }

        public async Task<ServiceMap> GetServiceMap()
        {
            var mongoCollection = _database.GetCollection<ServiceMapDbo>(StatusDbName);
            var mapDbo = await mongoCollection.FindSync(e => e.Id == nameof(ServiceMap)).SingleOrDefaultAsync();
            var services = mapDbo?.Services.Select(s =>
                s.IsReachable
                    ? MicrowaveServiceNode.ReachableMicrowaveServiceNode(s.ServiceEndPoint,s.Services)
                    : MicrowaveServiceNode.UnreachableMicrowaveServiceNode(s.ServiceEndPoint, s.Services));
            return mapDbo == null ? null : new ServiceMap(services);
        }

        public async Task SaveServiceMap(ServiceMap map)
        {
            var serviceMapDbo = new ServiceMapDbo
            {
                Services = map.AllServices.Select(s => new ServiceNodeWithDependentServicesDbo
                {
                    ServiceEndPoint = s.ServiceEndPoint,
                    Services = s.ConnectedServices,
                    IsReachable = s.IsReachable
                })
            };

            await InsertOrUpdate(serviceMapDbo);
        }
    }

    internal interface IIdentifiable
    {
        string Id { get; }
    }

    public class EventLocationDbo : IIdentifiable
    {
        public IEnumerable<MicrowaveServiceNode> Services { get; set; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; set; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; set; }
        public string Id => nameof(EventLocation);
    }

    public class ServiceMapDbo : IIdentifiable
    {
        public IEnumerable<ServiceNodeWithDependentServicesDbo> Services { get; set; }
        public string Id => nameof(ServiceMap);
    }

    public class ServiceNodeWithDependentServicesDbo
    {
        public ServiceEndPoint ServiceEndPoint { get; set; }
        public IEnumerable<ServiceEndPoint> Services { get; set; }
        public bool IsReachable { get; set; }
    }
}