using DistributedCaching.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var host = builder.Configuration.GetSection("Redis")["Host"];
var port = builder.Configuration.GetSection("Redis")["Port"];
var redisConnectionString = $"{host}:{port}";

//IDistributedCache DB connection
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = redisConnectionString;
});

// Redis Stack Exchange DB connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisService = sp.GetService<RedisService>();
    return redisService!.GetConnectionMultiplexer;
});
builder.Services.AddSingleton(_ =>
{
    return new RedisService(builder.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();