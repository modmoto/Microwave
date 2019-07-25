using System;
using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDb : ICosmosDb
    {
       
        private readonly MicrowaveCosmosDb _configuration;

        public CosmosDb(MicrowaveCosmosDb configuration)
        {
            _configuration = configuration;
        }

        public DocumentClient GetCosmosDbClient()
        {
            return new DocumentClient(new Uri(_configuration.DatabaseUrl),
                _configuration.PrimaryKey);
        }
    }
}