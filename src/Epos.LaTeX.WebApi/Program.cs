using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi
{
    public class Program
    {
        public static void Main(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.AddConsole(configure => {
                        configure.TimestampFormat = "[MM-dd HH:mm:ss,fff K] ";
                    });
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();

                    string thePort = Environment.GetEnvironmentVariable("PORT");
                    if (thePort != null) {
                        webBuilder.UseUrls($"http://0.0.0.0:{thePort}");
                    } else {
                        string theLetsEncrypt = Environment.GetEnvironmentVariable("LE");
                        if (theLetsEncrypt != null) {
                            webBuilder.UseUrls("http://0.0.0.0:80", "https://0.0.0.0:443");
                        }
                    }
                })
                .Build()
                .Run();
    }
}
