using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Results;
using Microwave.EventStores.Ports;
using Microwave.Persistence.UnitTestsSetup;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class SnapshotRepoTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadAndSaveSnapshotWithGuidList(PersistenceLayerProvider layerProvider)
        {
            var repo = layerProvider.SnapShotRepository;
            var userSnapshot = new UserSnapshot();

            var entityId = Guid.NewGuid();
            var newGuid = Guid.NewGuid();

            userSnapshot.SetId(entityId.ToString());
            userSnapshot.AddGuid(newGuid);
            userSnapshot.AddGuid(newGuid);

            await repo.SaveSnapShot(new SnapShotWrapper<UserSnapshot>(userSnapshot, entityId.ToString(), 0));
            var snapShotResult = await repo.LoadSnapShot<UserSnapshot>(entityId.ToString());

            var entityGuids = snapShotResult.Value.Guids.ToList();
            Assert.AreEqual(entityId, snapShotResult.Value.Id);
            Assert.AreEqual(2, entityGuids.Count);
            Assert.AreEqual(newGuid, entityGuids[0]);
            Assert.AreEqual(newGuid, entityGuids[1]);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadWithNull(PersistenceLayerProvider layerProvider)
        {
            var repo = layerProvider.SnapShotRepository;
            var snapShotResult = await repo.LoadSnapShot<UserSnapshotWithoutPrivateSetters>(null);

            Assert.IsTrue(snapShotResult.Is<NotFound>());
            Assert.ThrowsException<NotFoundException>(() => snapShotResult.Value);
        }
    }

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

    public class UserSnapshotWithoutPrivateSetters : Entity
    {
        public UserSnapshotWithoutPrivateSetters(string id, IEnumerable<Guid> guids)
        {
            Id = id;
            Guids = guids;
        }

        public UserSnapshotWithoutPrivateSetters()
        {
        }

        public string Id { get; }

        public IEnumerable<Guid> Guids { get; }
    }
}