using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using Epos.CmdLine;

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
                        "http://localhost:5000/api/latex"
                },
                new CmdLineOption<string>(
                    'c', "Sets the text color."
                ) {
                    LongName = "text-color",
                    DefaultValue = "FFFFFF"
                }
            },
            CmdLineFunc = Execute
        };

        public class Options
        {
            [CmdLineOption('u')]
            public string WebApiUrl { get; set; }

            [CmdLineOption('c')]
            public string TextColor { get; set; }
        }

        public static int Execute(Options options, CmdLineDefinition definition) {
            Console.WriteLine("Service URL: " + options.WebApiUrl);
            Console.WriteLine("Text color: " + options.TextColor);
            Console.WriteLine();
            Console.WriteLine("Please enter a snippet of LaTeX and finish with [Ctrl+Z][Enter].");

            using var theClient = new HttpClient();

            string theLaTeX = Console.In.ReadToEnd();
            var theRequest = new LaTeXServiceRequest {
                LaTeX = theLaTeX,
                TextColor = options.TextColor,
                PageColor = "000000"
            };

            string theJson = JsonSerializer.Serialize(
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
