using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Matrix
{
    [BsonId]
    public ObjectId Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}