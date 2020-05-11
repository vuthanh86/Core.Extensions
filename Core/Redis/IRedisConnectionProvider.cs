using StackExchange.Redis;

namespace Core.Redis
{
    public interface IRedisConnectionProvider
    {
        IConnectionMultiplexer GetConnection(string connectionString);
        IConnectionMultiplexer PreserveAsyncOrderGetConnection(string connectionString);
        string GetConnectionString(string service);
    }
}
