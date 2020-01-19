using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KestrelWithHttps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostingConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json")
                .Build();

            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Loopback, 5001);
                            options.Listen(IPAddress.Loopback, 5002, listenOptions =>
                            {
                                listenOptions.UseHttps("localhost.pfx", "testpassword");
                            });

                            var address = IPAddress.Parse(hostingConfig["KestrelAddress"]);
                            var port = int.Parse(hostingConfig["KestrelPort"]);
                            var cert = hostingConfig["CertificateFileName"];
                            var password = hostingConfig["CertificatePassword"];
                        });
                        webBuilder.UseStartup<Startup>();
                    });
        }
    }
}