using StackExchange.Redis;

namespace NetCore.Extensions.Core.Redis
{
    public interface IRedisConnectionProvider
    {
        IConnectionMultiplexer GetConnection(string connectionString);
        IConnectionMultiplexer PreserveAsyncOrderGetConnection(string connectionString);
        string GetConnectionString(string service);
    }
}
