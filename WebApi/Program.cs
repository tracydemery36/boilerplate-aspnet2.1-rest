﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models.Configuration;

namespace WebApi
{
    public class Program
    {
        private static string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        private static string currentDir = Directory.GetCurrentDirectory();

        public static void Main(string[] args)
        {            
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddJsonFile($"hosting.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var uris = config.GetSection("server.urls").Value.Split(";").Select(u => new Uri(u)).ToList();

            var host = WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseContentRoot(currentDir)
                .UseIISIntegration()
                .UseKestrel( options => {
                    foreach(var uri in uris)
                    {
                        if (uri.ToString().Contains("https://"))    // create HTTPS endpoint explicitly
                        { 
                            options.Listen(System.Net.IPAddress.Any, uri.Port, listenOptions =>
                            {
                                listenOptions.UseHttps(LoadCertificate(config));
                            });
                        }
                    }
                })
                .UseStartup<Startup>()
                .Build();

            return host;
        }

        private static X509Certificate2 LoadCertificate(IConfiguration config)
        {
            var certificateSettings = config.GetSection(nameof(CertificateSettings));
            var certificateFileName = certificateSettings.GetValue<string>("filename");
            var certificatePassword = certificateSettings.GetValue<string>("password");

            return new X509Certificate2(certificateFileName, certificatePassword);
        }
    }
}