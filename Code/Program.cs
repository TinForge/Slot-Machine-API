using Microsoft.Extensions.Options;
using MongoDB.Driver;

Console.WriteLine("Starting...");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));

// Register IMongoDatabase as a scoped service
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});


// Register Services
builder.Services.AddScoped<BalanceService>();
builder.Services.AddScoped<SpinService>();

builder.Services.AddControllers();

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var mongoClient = services.GetRequiredService<IMongoClient>();

    try
    {
        // Attempt to connect to the server
        mongoClient.ListDatabaseNames(); // This will throw an exception if the connection is not successful
        Console.WriteLine("Connected successfully to MongoDB.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to MongoDB: {ex.Message}");
    }

    try
    {
        var balanceService = services.GetRequiredService<BalanceService>();
        await balanceService.InitializeBalanceAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during initialization: {ex.Message}");
    }

    try
    {
        var spinService = services.GetRequiredService<SpinService>();
        await spinService.InitializeMatrixAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during initialization: {ex.Message}");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.Run();