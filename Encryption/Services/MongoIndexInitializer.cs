using Encryption.Interfaces;
using Encryption.Models.MongoDB;
using MongoDB.Driver;

namespace Encryption.Services
{
    public class MongoIndexInitializer : IHostedService
    {
        private readonly IMongoDbContext _mongoDbContext;
        private readonly ILogger<MongoIndexInitializer> _logger;
        public MongoIndexInitializer(IMongoDbContext mongoDbContext, ILogger<MongoIndexInitializer> logger)
        {
            _mongoDbContext = mongoDbContext;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var shortCodeKeys = Builders<EncryptedLinkDocument>.IndexKeys.Ascending(doc => doc.ShortCode);
                var shortCodeIndexModel = new CreateIndexModel<EncryptedLinkDocument>(
                    shortCodeKeys,
                    new CreateIndexOptions { Unique = true });

                var userIdKeys = Builders<EncryptedLinkDocument>.IndexKeys.Ascending(doc => doc.UserId);
                var userIdIndexModel = new CreateIndexModel<EncryptedLinkDocument>(userIdKeys);

                await _mongoDbContext.EncryptedLinks.Indexes.CreateManyAsync(
                    new[] { shortCodeIndexModel, userIdIndexModel },
                    cancellationToken);

                _logger.LogInformation("MongoDB indexes initialized successfully.");
            }

            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize MongoDB indexes on startup.");
            }
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
