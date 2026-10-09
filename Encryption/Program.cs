using Encryption.Data;
using Encryption.Interfaces;
using Encryption.Models.SQL;
using Encryption.Services;
using Encryption.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlMaster")
        ?? "Server=localhost;Database=EncryptionDb;Trusted_Connection=True;TrustServerCertificate=True;";
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();