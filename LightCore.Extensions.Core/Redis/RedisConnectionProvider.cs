using System;
using Microsoft.Extensions.Options;
using NetCore.Extensions.Core.Collections;
using StackExchange.Redis;

namespace NetCore.Extensions.Core.Redis
{
    public class RedisConnectionProvider : IRedisConnectionProvider
    {
        public static class Services
        {
            public const string DefaultCaching = "CMS.Caching.Redis";
            public const string StateCaching = "CMS.Caching.Redis.State";
            public const string MetaData = "CMS.Caching.Redis.Metadata";
        }

        private static readonly LazyDictionary<string, ConnectionMultiplexer> ConnectionMultiplexers = new LazyDictionary<string, ConnectionMultiplexer>();
        private static readonly LazyDictionary<string, ConnectionMultiplexer> PreserveOrderConnectionMultiplexers = new LazyDictionary<string, ConnectionMultiplexer>();
        private readonly RedisSettings settings;
    
        public RedisConnectionProvider(IOptions<RedisSettings> redisSetting)
        {
            settings = redisSetting.Value;
        }

        public string GetConnectionString(string service)
        {
            string connName;

            if (!settings.ServiceConnections.TryGetValue(service,out connName) || string.IsNullOrEmpty(connName) )
            {
                throw new Exception($"Connection name is null or empty for: {service}");
            }

            string connStr;

            if (!settings.ConnectionStrings.TryGetValue(connName, out connStr) || string.IsNullOrEmpty(connStr))
            {
                throw new Exception($"Connection string is null or empty for: {service}");
            }

            return connStr;
        }

        public IConnectionMultiplexer GetConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var connectionMultiplexer = ConnectionMultiplexers.GetOrAdd(connectionString, cfg =>
            {
                var options = ConfigurationOptions.Parse(connectionString);
                var conn = ConnectionMultiplexer.Connect(options);
                
                if (settings.PreserveAsyncOrder.HasValue)
                    conn.PreserveAsyncOrder = settings.PreserveAsyncOrder.Value;

                return conn;
            });

            return connectionMultiplexer;
        }

         public IConnectionMultiplexer PreserveAsyncOrderGetConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var connectionMultiplexer = PreserveOrderConnectionMultiplexers.GetOrAdd(connectionString, cfg =>
            {
                var options = ConfigurationOptions.Parse(connectionString);
                var conn = ConnectionMultiplexer.Connect(options);
                conn.PreserveAsyncOrder = true;
                return conn;
            });

            return connectionMultiplexer;
        }
    }
}