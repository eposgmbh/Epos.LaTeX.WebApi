using System;
using System.IO;
using System.Text;
using System.Text.Json;

using Epos.LaTeX.WebApi.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Epos.LaTeX.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class LaTeXController : Controller
    {
        private readonly ILaTeXService myLaTeXService;
        private readonly ILogger<LaTeXController> myLogger;

        public LaTeXController(ILaTeXService laTeXService, ILogger<LaTeXController> logger) {
            myLaTeXService = laTeXService;
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

                LaTeXServiceResponse theResponse = myLaTeXService.GetPng(theRequest);

                if (theResponse.IsSuccessful) {
                    return File(theResponse.PngImageData, "image/png");
                }

                return File(CreateImageFromMessage(theResponse.ErrorMessage), "image/png");
            } catch (Exception theException) {
                return File(CreateImageFromMessage(theException.Message), "image/png");
            }
        }

        private byte[] CreateImageFromMessage(string message) {
            using var theImage = new Image<Rgba32>(1000, 500);

            // foreach (var theInstalledFont in SystemFonts.Collection.Families) {
            //     Console.WriteLine(theInstalledFont.Name);
            // }

            Font theFont = SystemFonts.CreateFont("Noto Mono", 12, FontStyle.Regular);

            var theTextGraphicsOptions = new TextGraphicsOptions(true) {
                // draw the text along the path wrapping at the end of the line
                WrapTextWidth = 1000
            };

            // lets generate the text as a set of vectors drawn along the path

            IPathCollection theGlyphs = TextBuilder.GenerateGlyphs(
                message, new PointF(10.0f, 10.0f),
                new RendererOptions(theFont, theTextGraphicsOptions.DpiX, theTextGraphicsOptions.DpiY) {
                    HorizontalAlignment = theTextGraphicsOptions.HorizontalAlignment,
                    TabWidth = theTextGraphicsOptions.TabWidth,
                    VerticalAlignment = theTextGraphicsOptions.VerticalAlignment,
                    WrappingWidth = theTextGraphicsOptions.WrapTextWidth,
                    ApplyKerning = theTextGraphicsOptions.ApplyKerning
                }
            );

            theImage.Mutate(ctx => ctx
                .Fill(Rgba32.White)
                .Fill((GraphicsOptions) theTextGraphicsOptions, Rgba32.Black, theGlyphs));

            using var theMemoryStream = new MemoryStream();
            theImage.SaveAsPng(theMemoryStream);

            return theMemoryStream.GetBuffer();
        }
    }
}
