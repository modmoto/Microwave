using System.Collections.Generic;
using System.Reflection;

namespace Microwave.Persistence.CosmosDb
{
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies();
    }
}