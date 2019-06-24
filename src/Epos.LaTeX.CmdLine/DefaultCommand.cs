using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using DalSoft.RestClient;

using Epos.CmdLine;
using Epos.CmdLine.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Epos.LaTeX.CmdLine
{
    public static class DefaultCommand
    {
        public static readonly CmdLineSubcommand<Options> Instance = new CmdLineSubcommand<Options>(
            "default", "Reads LaTeX from stdin and opens a corresponding png image."
        ) {
            Options = {
                new CmdLineOption<string>(
                    'u', "Sets the Web API URL."
                ) {
                    LongName = "web-api-url",
                    DefaultValue =
                        Environment.GetEnvironmentVariable("LATEX_SERVICE_WEB_API_URL") ??
                        "http://localhost/api/latex"
                }
            },
            CmdLineFunc = Execute
        };

        public class Options
        {
            [CmdLineOption('u')]
            public string WebApiUrl { get; set; } =
                Environment.GetEnvironmentVariable("LATEX_SERVICE_WEB_API_URL") ?? "http://localhost/api/latex";
        }

        public static int Execute(Options options, CmdLineDefinition definition) {
            using var theClient = new HttpClient();

            string theLaTeX = Console.In.ReadToEnd();
            var theRequest = new LaTeXServiceRequest {
                LaTeX = theLaTeX,
                TextColor = "FFFFFF",
                PageColor = "000000"
            };

            string theJson = JsonSerializer.ToString(
                theRequest,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            string theBase64UrlJson = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(theJson));

            byte[] theBytes = theClient.GetByteArrayAsync($"{options.WebApiUrl}/{theBase64UrlJson}").Result;

            string thePngFilename = Path.GetTempFileName() + ".png";
            File.WriteAllBytes(thePngFilename, theBytes);

            using var theProcess = new Process {
                StartInfo = new ProcessStartInfo(thePngFilename) {
                    UseShellExecute = true
                }
            };

            theProcess.Start();

            return 0;
        }
    }
}
