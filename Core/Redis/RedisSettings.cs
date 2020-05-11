using System.Collections.Generic;

namespace Core.Redis
{
    public class RedisSettings
    {
        public IDictionary<string, string> ServiceConnections { get; set; }
        public bool? PreserveAsyncOrder { get; set; }
        public IDictionary<string, string> ConnectionStrings { get; set; }
    }

    public enum RedisDatabase
    {
        Default = 0,
        Bus = 1,
        Metadata = 2,
        State = 3
    }
}
