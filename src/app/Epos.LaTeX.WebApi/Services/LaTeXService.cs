using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Epos.Utilities;

using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi.Services;

public class LaTeXService : ILaTeXService
{
    private const int PngDensity = 300;
    private const int CacheCapacity = 300;

    private static readonly string Preamble = LoadResourceString("Epos.LaTeX.WebApi.Resources.Preamble.tex");
    private static readonly string End = LoadResourceString("Epos.LaTeX.WebApi.Resources.End.tex");

    private readonly ILogger<LaTeXService> myLogger;
    private readonly Cache<LaTeXServiceRequest, LaTeXServiceResponse> myCache = new(capacity: CacheCapacity);

    public LaTeXService(ILogger<LaTeXService> logger) {
        myLogger = logger;
        myCache = new Cache<LaTeXServiceRequest, LaTeXServiceResponse>(capacity: CacheCapacity);
    }

    private static string WorkingDirectory => Path.Combine(Path.GetTempPath(), "Epos.LaTeX.WebApi");

    public LaTeXServiceResponse GetArtifact(LaTeXServiceRequest request) {
        if (request == null) {
            throw new ArgumentNullException(nameof(request));
        }

        var theResponse = myCache[request];
        if (theResponse is not null) {
            myLogger.LogInformation($"Cache hit: {request}");
            return theResponse;
        }

        theResponse = request.Pdf
            ? GetPdfFile(request)
            : GetPngFile(request);

        myCache[request] = theResponse;

        return theResponse;
    }

    private LaTeXServiceResponse GetPngFile(LaTeXServiceRequest request) {
        LogRequestInformation(request);
        EnsureWorkingDirectoryExists();

        var thePdfFileResult = GetPdfFile(request);

        if (!thePdfFileResult.IsSuccessful) {
            return thePdfFileResult;
        }

        string thePdfFilePath = thePdfFileResult.PdfFilePath;
        string theFilenameWithoutExtension = Path.GetFileNameWithoutExtension(thePdfFilePath);
        string thePngFilePath = $"{Path.Combine(Path.GetDirectoryName(thePdfFilePath), theFilenameWithoutExtension)}.png";

        string args = request.RenderMode == RenderMode.Document
            ? $"-density {PngDensity} -define colorspace:auto-grayscale=false -append {thePdfFilePath} {thePngFilePath}"
            : $"-density {PngDensity} -define colorspace:auto-grayscale=false -chop 0x30 {thePdfFilePath} {thePngFilePath}";

        using var theConvertProcess = new Process {
            StartInfo = {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                FileName = "convert",
                WorkingDirectory = WorkingDirectory,
                Arguments = args
            }
        };
        theConvertProcess.Start();
        theConvertProcess.WaitForExit();

        LaTeXServiceResponse theResponse;
        if (File.Exists(thePngFilePath)) {
            theResponse = new LaTeXServiceResponse {
                IsSuccessful = true,
                PngImageData = File.ReadAllBytes(thePngFilePath)
            };
        } else {
            theResponse = new LaTeXServiceResponse {
                IsSuccessful = false,
                ErrorMessage = "PNG file cannot be created.",
            };
        }

        DeleteFilesInsideWorkingDirectory(theFilenameWithoutExtension);

        return theResponse;
    }

    private LaTeXServiceResponse GetPdfFile(LaTeXServiceRequest request) {
        LogRequestInformation(request);
        EnsureWorkingDirectoryExists();

        string theLaTeXDocument = GetLaTeXString(request);

        string theLaTeXFilePathWithoutExtension = $"{Path.Combine(WorkingDirectory, Guid.NewGuid().ToString("N"))}";
        string theLaTexFilePath = $"{theLaTeXFilePathWithoutExtension}.tex";

        File.WriteAllText(theLaTexFilePath, theLaTeXDocument, Encoding.UTF8);

        using var thePdflatexProcess = new Process {
            StartInfo = {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                FileName = "pdflatex",
                WorkingDirectory = WorkingDirectory,
                Arguments = theLaTexFilePath + " -no-c-style-errors -enable-installer -halt-on-error"
            }
        };
        thePdflatexProcess.Start();

        string theOutputString = thePdflatexProcess.StandardOutput.ReadToEnd();
        thePdflatexProcess.WaitForExit();

        myLogger.LogInformation($"Output (pdflatex):{Environment.NewLine}{theOutputString}");
        myLogger.LogInformation(string.Empty);

        LaTeXServiceResponse theResponse;

        int theFirstErrorIndex = theOutputString.IndexOf('!');
        if (theFirstErrorIndex != -1) {
            string theErrorMessage = theOutputString.Substring(theFirstErrorIndex + 2);
            theFirstErrorIndex = theErrorMessage.IndexOf('!');
            theErrorMessage = theErrorMessage.Substring(0, theFirstErrorIndex);

            theResponse = new LaTeXServiceResponse {
                IsSuccessful = false,
                ErrorMessage = theErrorMessage
            };
        } else {
            string thePdfFilePath = $"{theLaTeXFilePathWithoutExtension}.pdf";

            if (File.Exists(thePdfFilePath)) {
                theResponse = request.Pdf
                    ? new LaTeXServiceResponse { IsSuccessful = true, PdfData = File.ReadAllBytes(thePdfFilePath) }
                    : new LaTeXServiceResponse { IsSuccessful = true, PdfFilePath = thePdfFilePath };
            } else {
                theResponse = new LaTeXServiceResponse { IsSuccessful = false, ErrorMessage = "PDF file cannot be created." };
            }
        }

        if (request.Pdf) {
            // If no PDF is requested, the pdf file is still needed to be converted to a PNG file,
            // so we do not delete the files right now
            DeleteFilesInsideWorkingDirectory(theLaTeXFilePathWithoutExtension);
        }

        return theResponse;
    }

    private static string GetLaTeXString(LaTeXServiceRequest request) {
        if (request.RenderMode == RenderMode.Document) {
            return ReplaceColors(request.LaTeX, request);
        } else {
            var theLaTeXString = new StringBuilder(ReplaceColors(Preamble, request));

            if (request.RenderMode == RenderMode.MathModeFragment) {
                theLaTeXString.AppendLine(@"\begin{align*}");
            }

            theLaTeXString.AppendLine(request.LaTeX.Trim());

            if (request.RenderMode == RenderMode.MathModeFragment) {
                theLaTeXString.AppendLine(@"\end{align*}");
            }

            theLaTeXString.Append(End);

            return theLaTeXString.ToString();
        }
    }

    private static string ReplaceColors(string contents, LaTeXServiceRequest request) {
        contents = contents.Replace("##FONTCOLOR##", request.TextColor);
        
        string thePageColor =
            request.PageColor == "transparent"
            ? @"\nopagecolor"
            : @"\definecolor{pagecolor}{HTML}{" + request.PageColor + "}";
        
        contents = contents.Replace("##PAGECOLOR##", thePageColor);

        return contents;
    }

    private static string LoadResourceString(string resourceName) {
        using Stream theStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using var theStreamReader = new StreamReader(theStream);
        return theStreamReader.ReadToEnd();
    }

    private static void DeleteFilesInsideWorkingDirectory(string filenameWithoutExtension) {
        try {
            Directory
                .GetFiles(WorkingDirectory, $"{filenameWithoutExtension}.*")
                .ForEach(filename => File.Delete(filename));
        } catch { }
    }

    private static void EnsureWorkingDirectoryExists() {
        if (!Directory.Exists(WorkingDirectory)) {
            Directory.CreateDirectory(WorkingDirectory);
        }
    }

    private void LogRequestInformation(LaTeXServiceRequest request) {
        myLogger.LogInformation($"LaTeX: {request.LaTeX}");
        myLogger.LogInformation($"Text color: {request.TextColor}");
        myLogger.LogInformation($"Page color: {request.PageColor}");
        myLogger.LogInformation($"RenderMode: {request.RenderMode}");
        myLogger.LogInformation($"Pdf: {request.Pdf}");
        myLogger.LogInformation(string.Empty);
    }
}
