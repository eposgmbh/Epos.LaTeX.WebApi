using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Epos.LaTeX.WebApi
{
    public class Program
    {
        public static void Main(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();

                    string thePort = Environment.GetEnvironmentVariable("PORT") ?? "80";
                    webBuilder.UseUrls($"http://0.0.0.0:{thePort}");
                })
                .Build()
                .Run();
    }
}
