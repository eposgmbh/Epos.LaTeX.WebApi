using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Epos.Utilities;

using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi.Services
{
    public class LaTeXService : ILaTeXService
    {
        private readonly Cache<LaTeXServiceRequest, LaTeXServiceResponse> myCache;

        private static string WorkingDirectory => Path.Combine(Path.GetTempPath(), "Epos.LaTeX.WebApi");
        private const int PngDensity = 300;

        private readonly ILogger<LaTeXService> myLogger;

        public LaTeXService(ILogger<LaTeXService> logger) {
            myLogger = logger;
            myCache = new Cache<LaTeXServiceRequest, LaTeXServiceResponse>(capacity: 100);
        }

        public LaTeXServiceResponse GetPng(LaTeXServiceRequest request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            var theResponse = myCache[request];
            if (theResponse != null) {
                myLogger.LogInformation("Cache hit!");
                return theResponse;
            }

            myLogger.LogInformation($"LaTeX: {request.LaTeX}");
            myLogger.LogInformation($"Text color: {request.TextColor}");
            myLogger.LogInformation($"Page color: {request.PageColor}");
            myLogger.LogInformation(string.Empty);

            var theStopwatch = Stopwatch.StartNew();

            if (!Directory.Exists(WorkingDirectory)) {
                Directory.CreateDirectory(WorkingDirectory);
            }

            string theLaTeXFilenameWithoutExtension = $"{Path.Combine(WorkingDirectory, Guid.NewGuid().ToString("N"))}";
            string theLaTexFilename = $"{theLaTeXFilenameWithoutExtension}.tex";

            Stream theStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Epos.LaTeX.WebApi.Resources.Preamble.tex");
            var theStreamReader = new StreamReader(theStream);
            string thePreamble = theStreamReader.ReadToEnd();
            theStreamReader.Close();

            thePreamble = thePreamble.Replace("##FONTCOLOR##", request.TextColor);
            thePreamble = thePreamble.Replace("##PAGECOLOR##", request.PageColor);

            theStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Epos.LaTeX.WebApi.Resources.End.tex");
            theStreamReader = new StreamReader(theStream);
            string theEnd = theStreamReader.ReadToEnd();
            theStreamReader.Close();

            string theContents = thePreamble + request.LaTeX.Trim() + Environment.NewLine + theEnd;
            myLogger.LogInformation($"Input (pdflatex):{Environment.NewLine}{theContents}");
            myLogger.LogInformation(string.Empty);

            File.WriteAllText(theLaTexFilename, theContents, Encoding.UTF8);

            using var thePdflatexProcess = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    FileName = "pdflatex",
                    WorkingDirectory = WorkingDirectory,
                    Arguments = theLaTexFilename + " -no-c-style-errors -enable-installer -halt-on-error"
                }
            };
            thePdflatexProcess.Start();

            string theOutputString = thePdflatexProcess.StandardOutput.ReadToEnd();
            thePdflatexProcess.WaitForExit();

            myLogger.LogInformation($"Output (pdflatex):{Environment.NewLine}{theOutputString}");
            myLogger.LogInformation(string.Empty);

            int theFirstErrorIndex = theOutputString.IndexOf('!');
            string theErrorMessage;
            if (theFirstErrorIndex != -1) {
                theErrorMessage = theOutputString.Substring(theFirstErrorIndex + 2);
                theFirstErrorIndex = theErrorMessage.IndexOf('!');
                theErrorMessage = theErrorMessage.Substring(0, theFirstErrorIndex);
            } else {
                string thePdfFilename = $"{theLaTeXFilenameWithoutExtension}.pdf";
                string thePngFilename = $"{theLaTeXFilenameWithoutExtension}.png";

                if (File.Exists(thePdfFilename)) {
                    using var theConvertProcess = new Process {
                        StartInfo = {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            FileName = "convert",
                            WorkingDirectory = WorkingDirectory,
                            Arguments =
                                $"-density {PngDensity} -chop 0x30 {thePdfFilename} {thePngFilename}"
                        }
                    };
                    theConvertProcess.Start();
                    theConvertProcess.WaitForExit();

                    if (File.Exists(thePngFilename)) {
                        theResponse = new LaTeXServiceResponse {
                            IsSuccessful = true,
                            PngImageData = File.ReadAllBytes(thePngFilename),
                            DurationMilliseconds = theStopwatch.ElapsedMilliseconds
                        };

                        try {
                            Directory
                                .GetFiles(
                                    WorkingDirectory,
                                    $"{Path.GetFileNameWithoutExtension(theLaTeXFilenameWithoutExtension)}.*"
                                )
                                .ForEach(filename => File.Delete(filename));
                        } catch {}

                        myCache[request] = theResponse;

                        return theResponse;
                    }

                    theErrorMessage = "PNG file cannot be created.";
                } else {
                    theErrorMessage = "PDF file cannot be created.";
                }
            }

            theResponse = new LaTeXServiceResponse {
                IsSuccessful = false,
                ErrorMessage = theErrorMessage,
                DurationMilliseconds = theStopwatch.ElapsedMilliseconds
            };

            try {
                Directory
                    .GetFiles(
                        WorkingDirectory,
                        $"{Path.GetFileNameWithoutExtension(theLaTeXFilenameWithoutExtension)}.*"
                    )
                    .ForEach(filename => File.Delete(filename));
            } catch {}

            myCache[request] = theResponse;

            return theResponse; 
        }
    }
}
