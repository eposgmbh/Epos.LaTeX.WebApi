namespace Epos.LaTeX.WebApi.Services;

public record LaTeXServiceRequest
{
    public string LaTeX { get; set; }

    public string TextColor { get; set; } = "000000";

    public string PageColor { get; set; } = "FFFFFF";

    public bool RawLaTeX { get; set; }

    public bool Pdf { get; set; }
}
