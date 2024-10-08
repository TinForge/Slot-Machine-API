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
            var initialBalance = new Balance { Amount = 1000m };
            await _balanceCollection.InsertOneAsync(initialBalance);
        }
    }

    public async Task<Balance> GetBalanceAsync()
    {
        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (balance != null)
            return balance;
        else
            throw new InvalidOperationException("Balance not initialized.");

    }

    public async Task<decimal> UpdateBalanceAsync(decimal newBalance)
    {
        if (newBalance <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (balance == null)
            throw new InvalidOperationException("Balance not initialized.");

        if (balance.Amount == newBalance)
            throw new InvalidOperationException("Balance is already specified amount.");

        var update = Builders<Balance>.Update.Set(b => b.Amount, newBalance);
        var filter = Builders<Balance>.Filter.Eq(b => b.Id, balance.Id);
        var result = await _balanceCollection.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException("Failed to update balance.");

        return newBalance;
    }
}