using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microwave.Domain;

namespace Microwave.Eventstores.Persistence.CosmosDb
{
    public class CosmosDb : ICosmosDb
    {
       
        private readonly IMicrowaveConfiguration _configuration;

        public CosmosDb(IMicrowaveConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DocumentClient GetCosmosDbClient()
        {
            return new DocumentClient(new Uri(_configuration.DatabaseConfiguration.ConnectionString),
                _configuration.DatabaseConfiguration.PrimaryKey);
        }
    }
}