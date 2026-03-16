using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UrlManagement.Api.Models;

public class UrlMapping
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public long NumericId { get; set; } 
    public string LongUrl { get; set; } = string.Empty;
    public string? ShortCode { get; set; }
}