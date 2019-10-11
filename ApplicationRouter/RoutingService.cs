using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationProxy
{
    public class RoutingService
    {
        private readonly ConcurrentDictionary<int, HttpClient> _httpClients = new ConcurrentDictionary<int, HttpClient>();
        private readonly List<Route> _routes = new List<Route>();
        private readonly ReaderWriterLockSlim _routerLock = new ReaderWriterLockSlim(); //Probably don't need heavy-handed locking, but this can be refined later.

        private HttpClient HttpClient() => _httpClients.GetOrAdd(Thread.CurrentThread.ManagedThreadId, (key) => new HttpClient());

        public void AddRoute(Route route)
        {
            _routerLock.EnterWriteLock();

            try
            {
                var existingPorts = _routes.Select(x => x.UpstreamPort).Distinct();

                if (!existingPorts.Any(x => x == route.UpstreamPort))
                {
                    route.WebHost = GetWebHost(route.UpstreamPort);
                    route.WebHost.RunAsync();
                }
                else
                {
                    route.WebHost = _routes.First(x => x.UpstreamPort == route.UpstreamPort).WebHost;
                }

                _routes.Add(route);
            }
            finally
            {
                _routerLock.ExitWriteLock();
            }
        }

        public IEnumerable<Route> GetRoutes()
        {
            _routerLock.EnterReadLock();

            try
            {
                return _routes.ToList();
            }
            finally
            {
                _routerLock.ExitReadLock();
            }
        }

        public Route GetRoute(Guid id)
        {
            _routerLock.EnterReadLock();

            try
            {
                return _routes.First(x => x.Id == id);
            }
            finally
            {
                _routerLock.ExitReadLock();
            }
        }

        public void DeleteRoute(Guid id)
        {
            _routerLock.EnterWriteLock();

            try
            {
                var route = _routes.First(x => x.Id == id);
                var port = route.UpstreamPort;
                var matchingRoutes = _routes.Where(x => x.UpstreamPort == port && x.Id != id);

                if (!matchingRoutes.Any())
                {
                    KillWebHost(port);
                }

                _routes.Remove(route);
            }
            finally
            {
                _routerLock.ExitWriteLock();
            }
        }

        public Task ProcessHttpContext(HttpContext httpContext)
        {
            _routerLock.EnterReadLock();
            var response = httpContext.Response;
            var host = httpContext.Request.Host;

            try
            {
                var candidateRoutes = _routes.Where(x => string.Equals(x.UpstreamHost, host.Host, StringComparison.OrdinalIgnoreCase));
                candidateRoutes = candidateRoutes.Where(x => x.UpstreamPort == host.Port);

                if (candidateRoutes.Count() == 0)
                {
                    throw new Exception($"Could not route request for {host.Host}:{host.Port}. No routes match request.");
                }

                if (candidateRoutes.Count() > 1)
                {
                    throw new Exception($"Could not route request for {host.Host}:{host.Port}. Multiple routes match request.");
                }

                var route = candidateRoutes.First();
                return ProxyHttpContext(httpContext, route);
            }
            finally
            {
                _routerLock.ExitReadLock();
            }
        }

        private Task ProxyHttpContext(HttpContext httpContext, Route route)
        {
            var request = httpContext.Request;

            switch (request.Method)
            {
                case "GET":
                    return ProxyHttpGet(httpContext, route);
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task ProxyHttpGet(HttpContext httpContext, Route route)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;
            var httpClient = HttpClient();
            httpClient.DefaultRequestHeaders.Clear();

            foreach(var header in request.Headers)
            {
                if (header.Key.ToLower() == "host")
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("host", $"{route.DownstreamHost}:{route.DownstreamPort}");
                    continue;
                }

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());
            }

            try
            {
                var downstreamPathBuilder = new UriBuilder();
                downstreamPathBuilder.Path = request.Path;
                downstreamPathBuilder.Query = request.QueryString.Value;
                downstreamPathBuilder.Host = route.DownstreamHost;
                downstreamPathBuilder.Port = route.DownstreamPort;
                downstreamPathBuilder.Scheme = route.DownstreamIsTls ? "https" : "http";
                var downstreamResponse = await httpClient.GetAsync(downstreamPathBuilder.Uri, HttpCompletionOption.ResponseHeadersRead);
                response.StatusCode = (int)downstreamResponse.StatusCode;

                foreach (var header in downstreamResponse.Headers)
                {
                    response.Headers.Add(header.Key, header.Value.ToString());
                }

                var stream = await downstreamResponse.Content.ReadAsStreamAsync();
                var contentEncodings = downstreamResponse.Content.Headers.ContentEncoding;

                if (contentEncodings.Contains("gzip"))
                {
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                }
                else if (contentEncodings.Contains("deflate"))
                {
                    stream = new DeflateStream(stream, CompressionMode.Decompress);
                }

                response.ContentType = downstreamResponse.Content.Headers.ContentType?.ToString();
                await stream.CopyToAsync(response.Body);
                response.Body.Close();
            }
            catch (Exception ex)
            {
                //Debugging
            }
        }

        private IWebHost GetWebHost(int port)
        {
            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureKestrel((context, options) =>
                {
                    options.ListenAnyIP(port);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(this);
                })
                .UseStartup<Startup>()
                .Build();
        }

        private void KillWebHost(int port)
        {
            var webHost = _routes.First(x => x.UpstreamPort == port).WebHost;
            webHost.StopAsync().Wait();
        }

        private class Startup
        {
            public void Configure(IApplicationBuilder builder) 
            {
                builder.UseHttpListenerMiddleware();
            }
        }

        
    }
}
