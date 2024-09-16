using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

public class SlotsService
{
    private readonly IMongoCollection<Balance> _balanceCollection;
    private readonly IMongoCollection<Matrix> _matrixCollection;

    public SlotsService(IMongoDatabase database)
    {
        _balanceCollection = database.GetCollection<Balance>("balance");
        _matrixCollection = database.GetCollection<Matrix>("matrix");
    }

    public async Task InitializeMatrixAsync()
    {
        var existingMatrix = await _matrixCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (existingMatrix == null)
        {
            var initialMatrix = new Matrix { Width = 5, Height = 3 };
            await _matrixCollection.InsertOneAsync(initialMatrix);
        }
    }

    public async Task<int[]> ConfigureMatrixAsync(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Size must be greater than zero.");

        var matrix = await _matrixCollection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (matrix == null)
            throw new InvalidOperationException("Matrix not initialized.");

        if (matrix.Width == width && matrix.Height == height)
            throw new InvalidOperationException("Matrix already configured to dimensions.");

        var update = Builders<Matrix>.Update.Set(m => m.Width, width).Set(m => m.Height, height);
        var filter = Builders<Matrix>.Filter.Eq(m => m.Id, matrix.Id);
        var result = await _matrixCollection.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException("Failed to update matrix.");

        return new int[] { width, height };
    }

    public async Task<SpinResponse> SpinAsync(decimal betAmount)
    {
        // Retrieve the current balance
        var balance = await _balanceCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (balance == null)
            throw new InvalidOperationException("Balance not initialized.");

        if (betAmount <= 0 || betAmount > balance.Amount)
            throw new ArgumentException("Invalid bet amount.");

        //balance.Amount -= betAmount;

        var matrix = await _matrixCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (matrix == null)
            throw new Exception("Matrix not initialized.");


        var resultMatrix = GenerateMatrix(matrix.Width, matrix.Height);
        int points = 0;
        points += CheckHorizontalLines(resultMatrix);
        points += CheckDiagonalLines(resultMatrix);
        decimal winnings = points * betAmount;

        decimal newBalance = balance.Amount - betAmount + winnings;

        // Update the balance in the database
        var update = Builders<Balance>.Update.Set(b => b.Amount, newBalance);
        await _balanceCollection.UpdateOneAsync(new BsonDocument(), update);

        int[][] jaggedArray = Enumerable.Range(0, resultMatrix.GetLength(0))
        .Select(i => Enumerable.Range(0, resultMatrix.GetLength(1)).Select(j => resultMatrix[i, j]).ToArray()).ToArray();

        return new SpinResponse() { Matrix = jaggedArray, Winnings = winnings, CurrentBalance = newBalance };
    }

    private int[,] GenerateMatrix(int width, int height)
    {
        int[,] matrix = new int[height, width];
        Random random = new Random();

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                matrix[i, j] = random.Next(0, 10);

        return matrix;
    }

    private int CheckHorizontalLines(int[,] matrix)
    {
        int count = 1;
        int points = 0;
        int lastValue = 0;
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int row = 0; row < rows; row++) //Iterate through every row
        {
            count = 1;  //Start at count of 1
            for (int col = 1; col < cols; col++) //Start at second column
            {
                lastValue = matrix[row, col - 1]; //Compare with previous value

                if (matrix[row, col] == lastValue) //Increment if matches
                {
                    count++;
                }
                else //If it doesn't match
                {
                    if (count > 2) //Check if is win line
                        points += count * lastValue;

                    break; //Move to next row
                }
            }

            if (count > 2) //In case a row completely matches
                points += count * lastValue;
        }
        return points;
    }

    private int CheckDiagonalLines(int[,] matrix)
    {
        int points = 0;
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (rows < 2 || cols < 3) return 0;

        for (int row = 0; row < rows; row++) // Start from each row in the first column
        {
            int count = 1;
            bool up = false;

            int i = row;
            int j = 0;
            int lastValue = matrix[i, j];

            while (j < cols - 1) // Iterate until the last column
            {
                // Determine direction change
                if (i == 0 && up)
                    up = false;
                else if (i == rows - 1 && !up)
                    up = true;

                // Move diagonally
                if (up)
                    i--;
                else
                    i++;

                j++;

                // Check bounds
                if (i < 0 || i >= rows)
                    break;

                if (matrix[i, j] == lastValue) // Increment if it matches
                {
                    count++;
                }
                else // If it doesn't match
                {
                    if (count > 2) // Check if it's a win line
                    {
                        points += count * lastValue;
                    }

                    // Reset count for new sequence
                    count = 1;
                }

                lastValue = matrix[i, j]; // Store last value
            }

            // Final check for the last diagonal
            if (count > 2)
            {
                points += count * lastValue;
            }
        }
        return points;
    }
}