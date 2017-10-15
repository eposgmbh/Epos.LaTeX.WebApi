using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Epos.Blog.LaTeX.Services
{
    public class LaTeXService : ILaTeXService
    {
        private static readonly string WorkingDirectory = Path.Combine(Path.GetTempPath(), "Epos.Blog.LaTeX");
        private const int PngDensity = 300;

        private readonly ILogger<LaTeXService> myLogger;

        public LaTeXService(ILogger<LaTeXService> logger) {
            myLogger = logger;
        }

        public LaTeXServiceResponse GetPng(LaTeXServiceRequest request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            myLogger.LogDebug(request.LaTeX);
            myLogger.LogDebug(request.TextColor);
            myLogger.LogDebug(string.Empty);

            var theStopwatch = Stopwatch.StartNew();

            if (!Directory.Exists(WorkingDirectory)) {
                Directory.CreateDirectory(WorkingDirectory);
            }

            string theLaTexFilenameWithoutExtension = Path.Combine(WorkingDirectory, Guid.NewGuid().ToString("N"));
            string theLaTexFilename = theLaTexFilenameWithoutExtension + ".tex";

            Stream theStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Epos.Blog.LaTeX.Resources.Preamble.tex");
            StreamReader theStreamReader = new StreamReader(theStream);
            string thePreamble = theStreamReader.ReadToEnd();
            theStreamReader.Close();

            thePreamble = thePreamble.Replace("##COLOR##", request.TextColor);

            theStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Epos.Blog.LaTeX.Resources.End.tex");
            theStreamReader = new StreamReader(theStream);
            string theEnd = theStreamReader.ReadToEnd();
            theStreamReader.Close();

            var theContents = thePreamble + request.LaTeX.Trim() + Environment.NewLine + theEnd;
            myLogger.LogDebug(theContents);
            myLogger.LogDebug(string.Empty);

            File.WriteAllText(theLaTexFilename, theContents);

            var theProcess = new Process {
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
            theProcess.Start();

            string theOutputString = theProcess.StandardOutput.ReadToEnd();
            myLogger.LogDebug(theOutputString);
            myLogger.LogDebug(string.Empty);

            theProcess.WaitForExit();

            int theFirstErrorIndex = theOutputString.IndexOf('!');
            string theErrorMessage;
            if (theFirstErrorIndex != -1) {
                theErrorMessage = theOutputString.Substring(theFirstErrorIndex + 2);
                theFirstErrorIndex = theErrorMessage.IndexOf('!');
                theErrorMessage = theErrorMessage.Substring(0, theFirstErrorIndex);
                theErrorMessage = theErrorMessage.Replace(Environment.NewLine, " ");
            } else {
                string thePdfFilepath = Path.Combine(
                    WorkingDirectory, theLaTexFilenameWithoutExtension + ".pdf"
                );

                if (File.Exists(thePdfFilepath)) {
                    theProcess = new Process {
                        StartInfo = {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            FileName = "convert",
                            WorkingDirectory = WorkingDirectory,
                            Arguments =
                                $"-density {PngDensity} {theLaTexFilenameWithoutExtension}.pdf {theLaTexFilenameWithoutExtension}.png"
                        }
                    };
                    theProcess.Start();
                    theProcess.WaitForExit();

                    string thePngFilepath = Path.Combine(
                        WorkingDirectory, theLaTexFilenameWithoutExtension + ".png"
                    );

                    if (File.Exists(thePngFilepath)) {
                        return new LaTeXServiceResponse {
                            IsSuccessful = true,
                            PngImageData = File.ReadAllBytes(thePngFilepath),
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
                DurationMilliseconds = theStopwatch.ElapsedMilliseconds };
        }
    }
}
