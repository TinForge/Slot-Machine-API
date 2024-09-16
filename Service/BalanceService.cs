using MongoDB.Bson;
using MongoDB.Driver;

public class BalanceService
{
    private readonly IMongoCollection<Balance> _balanceCollection;

    public BalanceService(IMongoDatabase database)
    {
        _balanceCollection = database.GetCollection<Balance>("balance");
    }

    public async Task InitializeBalanceAsync()
    {
        var existingBalance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (existingBalance == null)
        {
            var initialBalance = new Balance
            {
                Amount = 1000m // Initial balance
            };

            await _balanceCollection.InsertOneAsync(initialBalance);
        }
    }

    public async Task<Balance> GetBalanceAsync()
    {
        return await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
    }


    public async Task<decimal> UpdateBalanceAsync(decimal newBalance)
    {
        if (newBalance <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (balance == null)
            throw new InvalidOperationException("Balance not initialized.");

        var update = Builders<Balance>.Update.Set(b => b.Amount, newBalance);
        var filter = Builders<Balance>.Filter.Eq(b => b.Id, balance.Id);
        var result = await _balanceCollection.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException("Failed to update balance.");

        return newBalance;
    }
}