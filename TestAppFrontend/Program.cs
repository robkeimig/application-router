using System;
using System.Net;
using ApplicationRouter;

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
            };

            service.AddRoute(route2);

            var route3 = new Route()
            {
                UpstreamHost = "localhost",
                UpstreamPort = 9001,
                DownstreamHost = "localhost",
                DownstreamPort = 9000,
            };

            service.AddRoute(route3);
            new WebClient().DownloadFile("http://localhost:9001/100MB.bin", "test.bin");
            Console.ReadKey();
        }
    }
}
