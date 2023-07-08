namespace Epos.LaTeX.WebApi.Services;

public record LaTeXServiceResponse
{
    public bool IsSuccessful { get; set; }

    public string ErrorMessage { get; set; }

    public byte[] PngImageData { get; set; }

    public byte[] PdfData { get; set; }

    public string PdfFilePath { get; set; }
}
