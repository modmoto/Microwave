using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Discovery;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class StatusRepository : IStatusRepository
    {
        private readonly IMongoDatabase _database;
        private const string StatusDbName = "MicrowaveStatusCollection";

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

            var mongoCollection = _database.GetCollection<EventLocationDbo>(StatusDbName);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<EventLocationDbo>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<EventLocationDbo, bool>>) (e => e.Id == eventLocationDbo.Id),
                eventLocationDbo,
                findOneAndReplaceOptions);
        }

        private Guid EventLocationId => new Guid("78448B83-1BA9-44DF-935B-78EC9B3D1FA4");

        public async Task<IEventLocation> GetEventLocation()
        {
            var mongoCollection = _database.GetCollection<EventLocationDbo>(StatusDbName);
            var location = await mongoCollection.FindSync(e => e.Id == EventLocationId).SingleOrDefaultAsync();
            if (location == null) return null;
            return new EventLocation(location.Services, location.UnresolvedEventSubscriptions, location.UnresolvedReadModeSubscriptions);
        }
    }

    public class EventLocationDbo
    {
        public IEnumerable<MicrowaveService> Services { get; set; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; set; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; set; }
        public Guid Id { get; set; }
    }
}