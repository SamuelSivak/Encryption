using MongoDB.Driver;
using Encryption.Models.Mongo;
using Encryption.Interfaces;



namespace Encryption.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        { 
            string connectionString = configuration.GetConnectionString("MongoDb")
                ?? "mongodb://localhost:27017";

            string databaseName = configuration.GetValue<string>("MongoDbDatabaseName")
                ?? "EncryptionDb";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);

            CreateIndexes();

        }

        public IMongoCollection<EncryptedLinkDocument> EncryptedLinks =>
                _database.GetCollection<EncryptedLinkDocument>("EncryptedLinks");

        private void CreateIndexes()
        {
            var shortCodeKeys = Builders<EncryptedLinkDocument>.IndexKeys.Ascending(doc => doc.ShortCode);
            var shortCodeIndexModel = new CreateIndexModel<EncryptedLinkDocument>(
                shortCodeKeys,
                new CreateIndexOptions { Unique = true});

            var userIdKeys = Builders<EncryptedLinkDocument>.IndexKeys.Ascending(doc => doc.UserId);
            var userIdIndexModel = new CreateIndexModel<EncryptedLinkDocument>(userIdKeys);

            EncryptedLinks.Indexes.CreateMany(new[] { shortCodeIndexModel, userIdIndexModel });
        }
    }
}
