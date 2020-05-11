using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Threading
{
    public class ApiContext
    {
        private Dictionary<string, object> contextItems;
        public string UserId { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
        public string Client { get; set; }

        public ApiContext(string userId)
        {
            UserId = userId;
        }

        public ApiContext()
        {
        }

        public void SetContextItem<T>(string key, T value)
        {
            if (contextItems == null)
                contextItems = new Dictionary<string, object>();

            contextItems[key] = value;
        }

        public bool TryGetContextItem<T>(string key, out T value)
        {
            if (contextItems != null && contextItems.TryGetValue(key, out var obj))
            {
                value = (T) obj;
                return true;
            }

            value = default(T);
            return false;
        }
    }

    public interface IApiContextAccessor
    {
        ApiContext CurrentApiContext { get; set; }
    }

    public class ApiContextAccessor : IApiContextAccessor
    {
        private static readonly AsyncLocal<ApiContext> ApiContextCurrent = new AsyncLocal<ApiContext>();

        #region Implementation of IApiContextAccessor

        public ApiContext CurrentApiContext
        {
            get => ApiContextCurrent.Value;
            set => ApiContextCurrent.Value = value;
        }

        #endregion
    }

    public static class ApiContextAccessorExtensions
    {
        public static ApiContext GetOrCreate(this IApiContextAccessor apiContextAccessor, Func<ApiContext> fac = null)
        {
            if (apiContextAccessor.CurrentApiContext == null)
            {
                apiContextAccessor.CurrentApiContext = fac == null ? new ApiContext() : fac();
            }

            return apiContextAccessor.CurrentApiContext;
        }
    }
}