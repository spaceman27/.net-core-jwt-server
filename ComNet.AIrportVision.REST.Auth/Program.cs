using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace ComNet.AirportVision.REST.Auth
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddXmlFile(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "appsettings.xml"), optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
