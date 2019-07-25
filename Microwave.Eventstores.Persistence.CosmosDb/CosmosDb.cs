using System;
using System.Security;
using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDb : ICosmosDb
    {
        public DocumentClient GetCosmosDbClient()
        {
            return new DocumentClient(CosmosDbLocation, PrimaryKey);
        }

        public SecureString PrimaryKey { get; set; }

        public Uri CosmosDbLocation { get; set; }
    }
}