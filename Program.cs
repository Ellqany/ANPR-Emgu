using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ANPR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseContentRoot(Directory.GetCurrentDirectory())
                          .UseStartup<Startup>()
                          .UseKestrel(options =>
                          {
                              options.Limits.MaxRequestBodySize = long.MaxValue;
                          }).UseIISIntegration();
            });
    }
}
