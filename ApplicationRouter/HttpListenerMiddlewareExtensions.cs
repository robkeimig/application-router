using Microsoft.AspNetCore.Builder;

namespace ApplicationRouter
{
    public static class HttpListenerMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpListenerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpListenerMiddleware>();
        }
    }
}
