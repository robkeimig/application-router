using System;
using Microsoft.AspNetCore.Hosting;

namespace ApplicationProxy
{
    public class Route
    {
        public Route()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Port number to listen on for public requests.
        /// </summary>
        public int UpstreamPort { get; set; }

        /// <summary>
        /// The hostname to match on for inbound public requests.
        /// </summary>
        public string UpstreamHost { get; set; }

        /// <summary>
        /// The host to use for proxying matched requests.
        /// </summary>
        public string DownstreamHost { get; set; }

        /// <summary>
        /// The port of the downstream host to use.
        /// </summary>
        public int DownstreamPort { get; set; }

        public bool DownstreamIsTls { get; set; }

        /// <summary>
        /// If Protocol==Http, this is the AspNetCore web host for this route.
        /// </summary>
        public IWebHost WebHost { get; set; }

        /// <summary>
        /// If Protocol==Http, this is the optional TLS certificate.
        /// </summary>
        public byte[] Pkcs12CertificateBytes { get; set; }

        /// <summary>
        /// If Protocol==Http and a TLS ceriticate byte array is specified, this optionally specifies the TLS cerificate password.
        /// </summary>
        public string Pkcs12CeritifcatePassword { get; set; }
    }
}
