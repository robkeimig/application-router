using Microsoft.AspNetCore.Builder;

namespace ApplicationProxy
{
    public static class HttpListenerMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpListenerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpListenerMiddleware>();
        }
    }
}
