using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores.Ports;
using Microwave.Persistence.UnitTestSetupPorts;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class SnapshotRepoTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadAndSaveSnapshotWithGuidList(IPersistenceLayerProvider layerProvider)
        {
            var repo = layerProvider.SnapShotRepository;
            var userSnapshot = new UserSnapshot();

            var entityId = GuidIdentity.Create(Guid.NewGuid());
            var newGuid = Guid.NewGuid();

            userSnapshot.SetId(entityId);
            userSnapshot.AddGuid(newGuid);
            userSnapshot.AddGuid(newGuid);

            await repo.SaveSnapShot(new SnapShotWrapper<UserSnapshot>(userSnapshot, entityId, 0));
            var snapShotResult = await repo.LoadSnapShot<UserSnapshot>(entityId);

            var entityGuids = snapShotResult.Entity.Guids.ToList();
            Assert.AreEqual(entityId.Id, snapShotResult.Entity.Id.Id);
            Assert.AreEqual(2, entityGuids.Count);
            Assert.AreEqual(newGuid, entityGuids[0]);
            Assert.AreEqual(newGuid, entityGuids[1]);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadAndSaveSnapshotWithoutPrivateSetters_DoesNotWork(IPersistenceLayerProvider layerProvider)
        {
            var repo = layerProvider.SnapShotRepository;
            var entityId = GuidIdentity.Create();
            var newGuid = Guid.NewGuid();
            var userSnapshot = new UserSnapshotWithoutPrivateSetters(entityId, new List<Guid> {newGuid, newGuid });


            await repo.SaveSnapShot(new SnapShotWrapper<UserSnapshotWithoutPrivateSetters>(userSnapshot, entityId, 0));
            var snapShotResult = await repo.LoadSnapShot<UserSnapshotWithoutPrivateSetters>(entityId);

            Assert.IsNull(snapShotResult.Entity.Guids);
            Assert.IsNull(snapShotResult.Entity.Id);
        }
    }

    public class UserSnapshot : Entity
    {
        public Identity Id { get; private set; }

        public IEnumerable<Guid> Guids { get; private set; } = new List<Guid>();

        public void AddGuid(Guid guid)
        {
            Guids = Guids.Append(guid);
        }

        public void SetId(Identity guid)
        {
            Id = guid;
        }
    }

    public class UserSnapshotWithoutPrivateSetters : Entity
    {
        public UserSnapshotWithoutPrivateSetters(Identity id, IEnumerable<Guid> guids)
        {
            Id = id;
            Guids = guids;
        }

        public UserSnapshotWithoutPrivateSetters()
        {
        }

        public Identity Id { get; }

        public IEnumerable<Guid> Guids { get; }
    }
}