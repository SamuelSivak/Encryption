using Encryption.Models.MongoDB;
using MongoDB.Driver;

namespace Encryption.Interfaces;

public interface IMongoDbContext
{
    IMongoCollection<EncryptedLinkDocument> EncryptedLinks { get; }
}