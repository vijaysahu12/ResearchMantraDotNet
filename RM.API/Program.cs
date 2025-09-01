using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RM.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    _ = webBuilder.UseStartup<Startup>()
                      .UseUrls("http://api.RM.site");
                });
        }
    }
}