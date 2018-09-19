using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace CHIS
{
    public class Program
    {

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>().UseNLog()
                .Build();

//        public static void Main(string[] args)
//        {
//            var host = new WebHostBuilder()
//                .UseKestrel()
//                .UseContentRoot(Directory.GetCurrentDirectory())
//                .UseIISIntegration();
//#if DEBUG
//            host = host.UseUrls("http://localhost:5000", "http://*:5000");
//#endif
//            host = host.UseStartup<Startup>()
//                .UseApplicationInsights();

//            host.Build().Run();
//        }
    }
}
