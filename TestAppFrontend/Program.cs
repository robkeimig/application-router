using System;
using System.Threading;
using ApplicationProxy;

namespace TestAppFrontend
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new RoutingService();

            var route = new Route()
            {
                UpstreamHost = "localhost",
                UpstreamPort = 8889,
                DownstreamHost = "speed.hetzner.de",
                DownstreamPort = 443,
                DownstreamIsTls = true
            };

            service.AddRoute(route);

            var route2 = new Route()
            {
                UpstreamHost = "localhost",
                UpstreamPort = 9000,
                DownstreamHost = "localhost",
                DownstreamPort = 8889,
                DownstreamIsTls = false
            };

            service.AddRoute(route2);

            var route3 = new Route()
            {
                UpstreamHost = "localhost",
                UpstreamPort = 9001,
                DownstreamHost = "localhost",
                DownstreamPort = 9000,
                DownstreamIsTls = false
            };

            service.AddRoute(route3);
            Console.ReadKey();
        }
    }
}
