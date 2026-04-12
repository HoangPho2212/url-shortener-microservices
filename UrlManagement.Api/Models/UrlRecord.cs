using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UrlManagement.Api.Models;

public class UrlRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? ShortUrl { get; set; }
    public string? OriginalUrl { get; set; }
    public string? CreatedBy { get; set; }
    public int Clicks { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
