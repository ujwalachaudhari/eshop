using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Helpers
{
    public class StorageAccountHelper
    {
        public string storageConnectionString;
        public string tableConnectionString;
        private CloudStorageAccount storageAccount;
        private CloudStorageAccount tableStorageAccount;

        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;

        public string StorageConnectionString
        {
            get { return storageConnectionString; }
            set
            {
                this.storageConnectionString = value;
                storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            }
        }

        public string TableConnectionString
        {
            get { return tableConnectionString; }
            set
            {
                this.tableConnectionString = value;
                tableStorageAccount = CloudStorageAccount.Parse(this.tableConnectionString);
            }
        }

        public async Task<string> UploadFileToBlobAsync(string filePath, string containerName)
        {
            blobClient = storageAccount.CreateCloudBlobClient();
            //Get the reference of the container
            var container = blobClient.GetContainerReference(containerName);

            //Check if the container already exists or not
            await container.CreateIfNotExistsAsync();

            BlobContainerPermissions permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };

            await container.SetPermissionsAsync(permissions);


            var fileName = Path.GetFileName(filePath);
            var blob = container.GetBlockBlobReference(fileName);

            await blob.DeleteIfExistsAsync();   ///delete the blob if already exists
            await blob.UploadFromFileAsync(filePath);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<CatalogEntity> SaveToTableAsync(CatalogItem item)
        {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                ImageUrl = item.ImageUrl,
                ReorderLevel = item.ReorderLevel,
                ManufacturingDate = item.ManufacturingDate,
                Quantity = item.Quantity,
                Price = item.Price
            };

            //tableClient = storageAccount.CreateCloudTableClient();
            tableClient = tableStorageAccount.CreateCloudTableClient();
            var catalogTable = tableClient.GetTableReference("catalog");

            await catalogTable.CreateIfNotExistsAsync();

            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);
            var tableResult = await catalogTable.ExecuteAsync(operation);

            return tableResult.Result as CatalogEntity;
        }
    }
}
