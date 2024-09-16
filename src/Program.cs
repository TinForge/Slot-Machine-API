using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<BalanceService>();
builder.Services.AddScoped<SlotsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var mongoClient = services.GetRequiredService<IMongoClient>();

    try
    {
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
        var spinService = services.GetRequiredService<SlotsService>();
        await spinService.InitializeMatrixAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during initialization: {ex.Message}");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });
}

//app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.Run();