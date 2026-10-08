using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Encryption.Models.MongoDB
{
    public class EncryptedLinkDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("shortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("encryptedUrlPayload")]
        public string EncryptedUrlPayload { get; set; } = string.Empty;

        [BsonElement("iv")]
        public string IV { get; set; } = string.Empty;

        [BsonElement("salt")]
        public string Salt { get; set; } = string.Empty;

        [BsonElement("authTag")]
        public string AuthTag { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("accessLogs")]
        public List<LinkAccessLog> AccessLogs { get; set; } = new();

    }
}
