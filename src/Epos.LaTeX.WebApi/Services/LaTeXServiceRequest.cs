namespace Epos.LaTeX.WebApi.Services
{
    public class LaTeXServiceRequest
    {
        public string LaTeX { get; set; }

        public string TextColor { get; set; } = "000000";

        public string PageColor { get; set; } = "FFFFFF";
    }
}
