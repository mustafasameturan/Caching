using StackExchange.Redis;

namespace DistributedCaching.Services;

public class RedisService
{
    private readonly ConnectionMultiplexer connectionMultiplexer;

    public RedisService(IConfiguration configuration)
    {
        var host = configuration.GetSection("Redis")["Host"];
        var port = configuration.GetSection("Redis")["Port"];

        var config = $"{host}:{port}";
        connectionMultiplexer = ConnectionMultiplexer.Connect(config);
    }

    public IDatabase GetDb(int db)
    {
        return connectionMultiplexer.GetDatabase(db);
    }

    public ConnectionMultiplexer GetConnectionMultiplexer => connectionMultiplexer;
}