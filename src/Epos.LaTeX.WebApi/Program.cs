using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi
{
    public class Program
    {
        public static void Main(string[] args) {
            Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(configure => {
                        configure.TimestampFormat = "[MM-dd HH:mm:ss,fff K] ";
                    });
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://0.0.0.0:5000");
                })
                .Build()
                .Run();
        }
    }
}
