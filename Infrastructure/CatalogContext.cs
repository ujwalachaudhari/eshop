using CatalogAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace CatalogAPI.Infrastructure
{
    public class CatalogContext
    {
        private IConfiguration configuration;
        private IMongoDatabase database;

        public CatalogContext(IConfiguration configuration)
        {
            this.configuration = configuration;
            var connectionString = configuration.GetValue<string>("MongoSettings:ConnectionString");


  //          string connectionString =
  //@"mongodb://testmongoapi:ImGmm6I47GItCTV2KnlaUybLAWKitWishbE7bFugOu7lIYEP8QXwxHp8CIvwEsLwj1GbMsMIoIXl2bq5679BFw==@testmongoapi.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@testmongoapi@";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            //var mongoClient = new MongoClient(settings);
            var client = new MongoClient(settings);


            //MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
            //MongoClient client = new MongoClient(settings);
            if (client != null)
            {
                this.database = client.GetDatabase(configuration.GetValue<string>("MongoSettings:Database"));
            }
        }

        public IMongoCollection<CatalogItem> Catalog
        {
            get 
            { 
                return this.database.GetCollection<CatalogItem>("products"); 
            }
        }
    }
}
