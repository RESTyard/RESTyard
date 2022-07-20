using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CarShack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .UseStartup<Startup>()
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                .Build();

            host.Run();
        }
    }
}
