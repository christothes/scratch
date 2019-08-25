namespace FoodTruckApi.DataRepos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public class CosmosDbRepository<T> : ICosmosDbRepository<T> where T : class
    {
        private readonly string DatabaseId;
        private readonly string CollectionId;
        private readonly IDocumentClient client;
        public CosmosDbRepository(IDocumentClient client, string databaseId, string collectionId)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            this.CollectionId = collectionId ?? throw new ArgumentNullException(nameof(collectionId));
        }

        public async Task<CosmosResult<T>> GetItemsAsync(Expression<Func<T, bool>> filter)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(filter)
                .AsDocumentQuery();

            var result = await query.ExecuteNextAsync<T>();
            var requestChargeTotal = result.RequestCharge;
            List<T> results = result.ToList();
            while (query.HasMoreResults)
            {
                result = await query.ExecuteNextAsync<T>();
                requestChargeTotal += result.RequestCharge;
                results.AddRange(result.ToList());
            }
            return new CosmosResult<T>(requestChargeTotal, results);
        }
    }
}