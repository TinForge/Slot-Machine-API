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
                Amount = (BsonDecimal128) 1000m // Initial balance
            };

            await _balanceCollection.InsertOneAsync(initialBalance);
        }
    }

    public async Task<Balance> GetBalanceAsync()
    {
        return await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
    }


    public async Task<decimal> UpdateBalanceAsync(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();

        // var documents = await _balanceCollection.Find(new BsonDocument()).ToListAsync();
        // Console.WriteLine($"Number of documents found: {documents.Count()}");
    
        // foreach (var doc in documents)
        //     Console.WriteLine($"Document ID: {doc.Id}, Amount: {doc.Amount}");

        if (balance == null)
            throw new InvalidOperationException("Balance not initialized.");

        var update = Builders<Balance>.Update.Inc(b => b.Amount, (BsonDecimal128) amount);
        var filter = Builders<Balance>.Filter.Eq(b => b.Id, balance.Id);
        var result = await _balanceCollection.UpdateOneAsync(filter, update);

         if (result.ModifiedCount == 0)
            throw new InvalidOperationException("Failed to update balance.");

        return (decimal)balance.Amount + amount;
    }
}