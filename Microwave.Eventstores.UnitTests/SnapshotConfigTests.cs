using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.EventStores;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class SnapshotConfigTests
    {
        [TestMethod]
        [DataRow("User", 0, 3, true)]
        [DataRow("User", 0, 4, true)]
        [DataRow("User", 1, 3, true)]
        [DataRow("User", 2, 4, true)]
        [DataRow("User", 3, 4, false)]
        [DataRow("User", 5, 5, false)]
        [DataRow("User", 6, 6, false)]
        [DataRow("NotFound", 2, 4, false)]
        public void SnapshotConfig(string className, long last, long current, bool expected)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            var snapShotConfig = new SnapShotConfig(config);

            var doesNeedSnapshot = snapShotConfig.DoesNeedSnapshot(className, last, current);
            Assert.AreEqual(expected, doesNeedSnapshot);
        }
    }
}