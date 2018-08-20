using System;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DependencyInjection.Framework.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAllLoadedQuerries()
        {
            var objectPersister = new ObjectPersister<TestQuerry>();
            var testQuerry = new TestQuerry
            {
                Name = "Twstjeah",
                Id = Guid.NewGuid()
            };
            await objectPersister.Save(testQuerry);

            var serviceCollection = (IServiceCollection) new ServiceCollection();
            serviceCollection.AddAllLoadedQuerries(typeof(TestQuerry).Assembly);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();
            var querryInDi = (TestQuerry) buildServiceProvider.GetService(typeof(TestQuerry));

            Assert.Equal(testQuerry.Id, querryInDi.Id);
            Assert.Equal(testQuerry.Name, querryInDi.Name );
        }
    }

    public class TestQuerry : Querry
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}