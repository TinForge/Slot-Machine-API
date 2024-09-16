using MongoDB.Bson;
using MongoDB.Driver;

public class BettingService
{
    private readonly IMongoCollection<Balance> _balanceCollection;

    public BettingService(IMongoDatabase database)
    {
        _balanceCollection = database.GetCollection<Balance>("balance");
    }

    public async Task<decimal> PlaceBetAsync(decimal betAmount)
    {
        // Retrieve the current balance
        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (balance == null)
            throw new InvalidOperationException("Balance not initialized.");

        if (betAmount <= 0 || betAmount > balance.Amount)
            throw new ArgumentException("Invalid bet amount.");

        // Randomly determine if the user wins or loses
        var random = new Random();
        bool isWin = random.Next(2) == 0; // 50% chance to win

        BsonDecimal128 newBalance = balance.Amount;

        if (isWin)
            newBalance = new BsonDecimal128((decimal)balance.Amount + betAmount);
        else
            newBalance = new BsonDecimal128((decimal)balance.Amount - betAmount);

        // Update the balance in the database
        var update = Builders<Balance>.Update.Set(b => b.Amount, newBalance);
        await _balanceCollection.UpdateOneAsync(new BsonDocument(), update);

        return (decimal)newBalance;
    }
}