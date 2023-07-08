using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Epos.LaTeX.WebApi.Services;

public class ErrorImageService : IErrorImageService
{
    public byte[] GetErrorImageFromMessage(string message) {
        using var theImage = new Image<Rgba32>(1000, 500);

        // foreach (var theInstalledFont in SystemFonts.Collection.Families) {
        //     Console.WriteLine(theInstalledFont.Name);
        // }

        Font theFont = SystemFonts.CreateFont("Noto Mono", 12, FontStyle.Regular);

        var theTextGraphicsOptions = new TextGraphicsOptions(true) {
            // draw the text along the path wrapping at the end of the line
            WrapTextWidth = 1000
        };

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
