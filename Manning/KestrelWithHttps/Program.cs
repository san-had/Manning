using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KestrelWithHttps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var hostingConfig = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("hosting.json")
                 .Build();

            return WebHost.CreateDefaultBuilder(args)
                //.UseKestrel()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5001);
                    options.Listen(IPAddress.Loopback, 5002, listenOptions =>
                    {
                        listenOptions.UseHttps("localhost.pfx", "testpassword");
                    });

                    var address = IPAddress.Parse(hostingConfig["KestrelAddress"]); var port =
                    int.Parse(hostingConfig["KestrelPort"]); var cert =
                    hostingConfig["CertificateFileName"]; var password =
                    hostingConfig["CertificatePassword"]; options.Listen(address, port, listenOptions
                    =>
                    { listenOptions.UseHttps(cert, password); });

                    var certificateFromStore = GetSslCertificate();
                    if (certificateFromStore != null)
                    {
                        options.Listen(IPAddress.Loopback, 5004, listenOptions =>
                        {
                            listenOptions.UseHttps(certificateFromStore);
                        });
                    }
                })
                .UseStartup<Startup>();
        }

        private static X509Certificate2 GetSslCertificate()
        {
            try
            {
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);

                foreach (var cert in certs)
                {
                    if (cert.FriendlyName == "ASP.NET Core Development")
                    {
                        store.Close();
                        return cert;
                    }
                }
                store.Close();
            }
            catch (PlatformNotSupportedException)
            {
                throw;
            }

            return null;
        }
    }
}