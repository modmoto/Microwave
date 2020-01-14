using System.Collections.Generic;
using System.Reflection;
using Microwave.Persistence.CosmosDb;
using Microwave.Persistence.UnitTestsSetup.MongoDb;

namespace Microwave.Persistence.UnitTestsSetup.CosmosDb
{
    public class AssemblyProvider : IAssemblyProvider
    {
        public IEnumerable<Assembly> GetAssemblies()
        {
            return  new List<Assembly>
            {
                Assembly.GetAssembly(typeof(TestEvent1))
            };
        }
    }
}