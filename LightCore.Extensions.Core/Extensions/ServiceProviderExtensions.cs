using System;

namespace NetCore.Extensions.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T Resolve<T>(this IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(T));
            if (service != null)
            {
                return (T)service;
            }

            return default(T);
        }
    }
}
