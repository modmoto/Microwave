using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microwave
{
    public interface IPersistenceLayer
    {
        IServiceCollection AddPersistenceLayer(IServiceCollection services, IEnumerable<Assembly> assemblies);
    }
}