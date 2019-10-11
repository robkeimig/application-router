using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApplicationRouter
{
    public class HttpListenerMiddleware
    {
        private readonly RoutingService _routingService;

        public HttpListenerMiddleware(RequestDelegate _, RoutingService routingService)
        {
            _routingService = routingService;
        }

        public Task Invoke(HttpContext httpContext)
        {
            try
            {
                return _routingService.ProcessHttpContext(httpContext);
            }
            catch
            {
                httpContext.Response.StatusCode = 503;
                httpContext.Response.Body.Close();
            }

            return Task.CompletedTask;
        }
    }
}
