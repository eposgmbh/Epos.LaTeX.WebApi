using System;
using System.Text;
using System.Text.Json;

using Epos.LaTeX.WebApi.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Epos.LaTeX.WebApi.Controllers;

[Route("api/[controller]")]
public class LaTeXController : Controller
{
    private readonly ILaTeXService myLaTeXService;
    private readonly IErrorImageService myErrorImageService;
    private readonly ILogger<LaTeXController> myLogger;

    public LaTeXController(
        ILaTeXService laTeXService, IErrorImageService errorImageService, ILogger<LaTeXController> logger
    ) {
        myLaTeXService = laTeXService;
        myErrorImageService = errorImageService;
        myLogger = logger;
    }

    // GET api/latex/{jsonBase64}
    [HttpGet("{jsonBase64Url}")]
    [ResponseCache(Duration = 31536000)]
    public IActionResult Get(string jsonBase64Url) {
        try {
            byte[] theBytes = WebEncoders.Base64UrlDecode(jsonBase64Url);
            string theJson = Encoding.UTF8.GetString(theBytes);

            LaTeXServiceRequest theRequest = JsonSerializer.Deserialize<LaTeXServiceRequest>(
                theJson, new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );

            LaTeXServiceResponse theResponse = myLaTeXService.GetArtifact(theRequest);

            if (theResponse.IsSuccessful) {
                return theResponse.PngImageData is not null
                    ? File(theResponse.PngImageData, "image/png")
                    : File(theResponse.PdfData, "application/pdf");
            }

            return File(myErrorImageService.GetErrorImageFromMessage(theResponse.ErrorMessage), "image/png");
        } catch (Exception theException) {
            if (theException is not JsonException) {
                myLogger.LogWarning(theException.ToString());
            }

            return File(myErrorImageService.GetErrorImageFromMessage(theException.Message), "image/png");
        }
    }
}
