using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi.Services
{
    public class LaTeXService : ILaTeXService
    {
        private static string WorkingDirectory => Path.Combine(Path.GetTempPath(), "Epos.LaTeX.WebApi");
        private const int PngDensity = 300;

        private readonly ILogger<LaTeXService> myLogger;

        public LaTeXService(ILogger<LaTeXService> logger) {
            myLogger = logger;
        }

        public LaTeXServiceResponse GetPng(LaTeXServiceRequest request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            myLogger.LogWarning($"LaTeX: {request.LaTeX}");
            myLogger.LogWarning($"Text color: {request.TextColor}");
            myLogger.LogWarning($"Page color: {request.PageColor}");
            myLogger.LogWarning(string.Empty);

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
            myLogger.LogWarning($"Input (pdflatex):{Environment.NewLine}{theContents}");
            myLogger.LogWarning(string.Empty);

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

            myLogger.LogWarning($"Output (pdflatex):{Environment.NewLine}{theOutputString}");
            myLogger.LogWarning(string.Empty);

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
                        return new LaTeXServiceResponse {
                            IsSuccessful = true,
                            PngImageData = File.ReadAllBytes(thePngFilename),
                            DurationMilliseconds = theStopwatch.ElapsedMilliseconds
                        };
                    }

                    theErrorMessage = "PNG file cannot be created.";
                } else {
                    theErrorMessage = "PDF file cannot be created.";
                }
            }

            return new LaTeXServiceResponse {
                IsSuccessful = false,
                ErrorMessage = theErrorMessage,
                DurationMilliseconds = theStopwatch.ElapsedMilliseconds
            };
        }
    }
}
