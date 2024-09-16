using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Balance
{
    [BsonId]
    public ObjectId Id { get; set; }
    public decimal Amount { get; set; }
}
