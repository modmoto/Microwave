using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class SnapshotRepoTests : IntegrationTests
    {
        [TestMethod]
        public async Task LoadAndSaveSnapshotWithGuidList()
        {
            var repo = new SnapShotRepository(EventDatabase);
            var userSnapshot = new UserSnapshot();

            var entityId = Guid.NewGuid();
            var newGuid = Guid.NewGuid();

            userSnapshot.SetId(entityId.ToString());
            userSnapshot.AddGuid(newGuid);
            userSnapshot.AddGuid(newGuid);

            await repo.SaveSnapShot(new SnapShotWrapper<UserSnapshot>(userSnapshot, entityId.ToString(), 0));
            var snapShotResult = await repo.LoadSnapShot<UserSnapshot>(entityId.ToString());

            var entityGuids = snapShotResult.Entity.Guids.ToList();
            Assert.AreEqual(entityId.ToString(), snapShotResult.Entity.Id);
            Assert.AreEqual(2, entityGuids.Count);
            Assert.AreEqual(newGuid, entityGuids[0]);
            Assert.AreEqual(newGuid, entityGuids[1]);
        }
    }

    [SnapShotAfter(3)]
    public class UserSnapshot : Entity
    {
        public string Id { get; private set; }

        public IEnumerable<Guid> Guids { get; private set; } = new List<Guid>();

        public void AddGuid(Guid guid)
        {
            Guids = Guids.Append(guid);
        }

        public void SetId(string guid)
        {
            Id = guid;
        }
    }
}