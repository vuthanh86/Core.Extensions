using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetCore.Extensions.Core.Threading;

namespace NetCore.Extensions.Core.AspNet.Middlewares
{
    public class ApiContextMiddleware
    {
        private readonly RequestDelegate next;

        public ApiContextMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ILogger logger, IApiContextAccessor contextAccessor /* other scoped dependencies */)
        {
            context.Request.Headers.TryGetValue("UserId", out var uid);
            contextAccessor.CurrentApiContext = new ApiContext(uid);

            await next(context);
        }
    }
}
