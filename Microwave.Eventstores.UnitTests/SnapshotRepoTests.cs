
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class SnapshotRepoTests
    {
        [TestMethod]
        public async Task LoadAndSaveSnapshotWithGuidList()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("LoadAndSaveSnapshotWithGuidList")
                .Options;

            var eventStoreContext = new EventStoreContext(options);
            var repo = new SnapShotRepository(eventStoreContext, new ObjectConverter());
            var userSnapshot = new UserSnapshot();

            var entityId = Guid.NewGuid();
            var newGuid = Guid.NewGuid();

            userSnapshot.SetId(entityId);
            userSnapshot.AddGuid(newGuid);
            userSnapshot.AddGuid(newGuid);

            await repo.SaveSnapShot(userSnapshot, entityId, 0);
            var snapShotResult = await repo.LoadSnapShot<UserSnapshot>(entityId);

            var entityGuids = snapShotResult.Entity.Guids.ToList();
            Assert.AreEqual(entityId, snapShotResult.Entity.Id);
            Assert.AreEqual(2, entityGuids.Count);
            Assert.AreEqual(newGuid, entityGuids[0]);
            Assert.AreEqual(newGuid, entityGuids[1]);
        }
    }

    [SnapShotAfter(3)]
    public class UserSnapshot : Entity
    {
        public Guid Id { get; private set; }

        public IEnumerable<Guid> Guids { get; private set; } = new List<Guid>();

        public void AddGuid(Guid guid)
        {
            Guids = Guids.Append(guid);
        }

        public void SetId(Guid guid)
        {
            Id = guid;
        }
    }
}