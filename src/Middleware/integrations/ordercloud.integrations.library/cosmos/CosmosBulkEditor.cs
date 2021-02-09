using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using ordercloud.integrations.library;
using Cosmonaut.Attributes;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System.Reflection;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;

namespace ordercloud.integrations.library
{
    public interface ICosmosBulkOperations
	{
        Task<List<T>> GetAllAsync<T>(string collectionName);
        Task ImportAsync<T>(string collectionName, List<T> toImport) where T : CosmosObject;
        Task Delete<T>(string collectionName, List<T> toDelete) where T : CosmosObject;
        Task UpdateAllAsync<T>(string collectionName, Func<JObject, List<UpdateOperation>> updateFunc) where T : CosmosObject;
    }

    // https://github.com/Azure/azure-cosmosdb-bulkexecutor-dotnet-getting-started/blob/master/BulkUpdateSample/BulkUpdateSample/Program.cs
    public class CosmosBulkOperations : ICosmosBulkOperations
    {
        private readonly string DatabaseName;
        private readonly DocumentClient client;

        public CosmosBulkOperations(CosmosConfig config)
        {
            DatabaseName = config.DatabaseName;
            client = new DocumentClient(new Uri(config.EndpointUri), config.PrimaryKey, new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                RequestTimeout = config.RequestTimeout
            });
        }

        public async Task ImportAsync<T>(string collectionName, List<T> toImport) where T : CosmosObject
		{
            int batchSize = 1000;

            var bulkExecutor = await BuildClientAsync(collectionName);

            var partitionKeyProperty = GetPartitionKeyProp<T>();

            var batchedImportItems = toImport.Chunk(batchSize).ToList();

            Console.WriteLine($"\nImporting {toImport.Count} Documents Batches of {batchedImportItems.Count}. Beginning.");

            await Task.Run(async () =>
            {
                // Prepare for bulk update.
                var batchesRun = 0;
                long totalNumberOfDocumentsImported = 0;
                BulkImportResponse bulkImportResponse = null;
                do
                {
                    try
                    {
                        bulkImportResponse = await bulkExecutor.BulkImportAsync(
                                documents: batchedImportItems[batchesRun],
                                enableUpsert: true,
                                disableAutomaticIdGeneration: true,
                                maxConcurrencyPerPartitionKeyRange: null,
                                maxInMemorySortingBatchSize: null,
                                cancellationToken: new CancellationTokenSource().Token);
                    }
                    catch (DocumentClientException de)
                    {
                        Console.WriteLine("Document client exception: {0}", de);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e);
                    }

                    LogProgress(bulkImportResponse);
                    batchesRun++;
                    totalNumberOfDocumentsImported += bulkImportResponse.NumberOfDocumentsImported;
                } while (totalNumberOfDocumentsImported < toImport.Count);
            });
        }

        public async Task Delete<T>(string collectionName, List<T> toDelete) where T : CosmosObject
        {
            var bulkExecutor = await BuildClientAsync(collectionName);

            var partitionKeyProperty = GetPartitionKeyProp<T>();
            
            var deleteOperations = toDelete.Select(doc => {
                var partitionKeyValue = (string) typeof(T).GetProperty(partitionKeyProperty.Name).GetValue(doc, null);
                    return new Tuple<string, string>(partitionKeyValue, doc.id); 
             }).ToList();

            BulkDeleteResponse bulkDeleteResponse = null;
            try
            {
                bulkDeleteResponse = await bulkExecutor.BulkDeleteAsync(
                    deleteOperations,
                    deleteBatchSize: 1000,
                    cancellationToken: new CancellationTokenSource().Token
                    );
            }
            catch (DocumentClientException de)
            {
                Trace.TraceError("Document client exception: {0}", de);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception: {0}", e);
            }

            Trace.WriteLine("\n\n--------------------------------------------------------------------- ");
            Trace.WriteLine("Executing bulk delete:");
            Trace.WriteLine("--------------------------------------------------------------------- ");
            Trace.WriteLine("\n\nOverall summary of bulk delete:");
            Trace.WriteLine("--------------------------------------------------------------------- ");
            Trace.WriteLine(String.Format("Deleted {0} docs", bulkDeleteResponse.NumberOfDocumentsDeleted));
            Trace.WriteLine("--------------------------------------------------------------------- \n");
        }

        public async Task UpdateAllAsync<T>(string collectionName, Func<JObject, List<UpdateOperation>> updateFunc) where T : CosmosObject
        {
            int batchSize = 1000;

            var bulkExecutor = await BuildClientAsync(collectionName);
            
            // Generate update items.
            var documents = await GetAllAsync<JObject>(collectionName);

            var partitionKeyProperty = GetPartitionKeyProp<T>();

            var updateItems = documents.Select(doc =>
            {
                var id = doc.Value<string>("id");
                var partitionKeyValue = doc.Value<string>(partitionKeyProperty.Name);
                return new UpdateItem(id, partitionKeyValue, updateFunc(doc));
            }).Where(ui => ui.PartitionKey != null).ToList();


            var batchedUpdateItems = updateItems.Chunk(batchSize).ToList();

            Console.WriteLine(String.Format("\nFound {0} Documents to update. {1} Batches of {2}. Beginning.", documents.Count, batchedUpdateItems.Count, batchSize));

            await Task.Run(async () =>
            {
                // Prepare for bulk update.
                var batchesRun = 0;
                long totalNumberOfDocumentsUpdated = 0;
                BulkUpdateResponse bulkUpdateResponse = null;
                do
                {
                    try
                    {
                        bulkUpdateResponse = await bulkExecutor.BulkUpdateAsync(
                            updateItems: batchedUpdateItems[batchesRun],
                            maxConcurrencyPerPartitionKeyRange: null,
                            cancellationToken: new CancellationTokenSource().Token);
                    }
                    catch (DocumentClientException de)
                    {
                        Console.WriteLine("Document client exception: {0}", de);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e);
                    }

                    LogProgress(bulkUpdateResponse);
                    batchesRun++;
                    totalNumberOfDocumentsUpdated += bulkUpdateResponse.NumberOfDocumentsUpdated;
                } while (totalNumberOfDocumentsUpdated < updateItems.Count);
            });
        }

        public async Task<List<T>> GetAllAsync<T>(string collectionName)
        {
            var collection = GetCollectionIfExists(client, DatabaseName, collectionName);
            var list = new List<T>();
            using (var queryable = client.CreateDocumentQuery<T>(collection.SelfLink).AsDocumentQuery())
            {
                while (queryable.HasMoreResults)
                {
                    var batch = await queryable.ExecuteNextAsync<T>();
                    list = list.Concat(batch).ToList();
                }
            }
            return list;
        }

        private PropertyInfo GetPartitionKeyProp<TDoc>() where TDoc : CosmosObject
		{
           return typeof(TDoc).GetProperties().FirstOrDefault(prop => prop.HasAttribute<CosmosPartitionKeyAttribute>());
        }

        private static DocumentCollection GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            var database = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
            if (database == null) return null;

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
                .Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

        private void LogProgress(BulkUpdateResponse response)
        {
            Console.WriteLine(String.Format("\nSummary for collection"));
            Console.WriteLine("--------------------------------------------------------------------- ");
            Console.WriteLine(String.Format("Updated {0} docs @ {1} updates/s, {2} RU/s in {3} sec",
                response.NumberOfDocumentsUpdated,
                Math.Round(response.NumberOfDocumentsUpdated / response.TotalTimeTaken.TotalSeconds),
                Math.Round(response.TotalRequestUnitsConsumed / response.TotalTimeTaken.TotalSeconds),
                response.TotalTimeTaken.TotalSeconds));
            Console.WriteLine(String.Format("Average RU consumption per document update: {0}",
                (response.TotalRequestUnitsConsumed / response.NumberOfDocumentsUpdated)));
            Console.WriteLine("---------------------------------------------------------------------\n ");
        }

        private void LogProgress(BulkImportResponse response)
        {
            Console.WriteLine(String.Format("\nSummary for collection"));
            Console.WriteLine("--------------------------------------------------------------------- ");
            Console.WriteLine(String.Format("Created {0} docs @ {1} writes/s, {2} RU/s in {3} sec",
                response.NumberOfDocumentsImported,
                Math.Round(response.NumberOfDocumentsImported / response.TotalTimeTaken.TotalSeconds),
                Math.Round(response.TotalRequestUnitsConsumed / response.TotalTimeTaken.TotalSeconds),
                response.TotalTimeTaken.TotalSeconds));
            Console.WriteLine(String.Format("Average RU consumption per document update: {0}",
                (response.TotalRequestUnitsConsumed / response.NumberOfDocumentsImported)));
            Console.WriteLine("---------------------------------------------------------------------\n ");
        }

        private async Task<BulkExecutor> BuildClientAsync(string collectionName)
        {
            var collection = GetCollectionIfExists(client, DatabaseName, collectionName);
            var bulkExecutor = new BulkExecutor(client, collection);
            await bulkExecutor.InitializeAsync();

            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 10;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 5;
            return bulkExecutor;
        }
    }
}
