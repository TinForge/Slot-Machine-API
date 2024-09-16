using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Balance
{
    [BsonId]
    public ObjectId Id { get; set; }
    public BsonDecimal128 Amount { get; set; }
}
