using System;

namespace Epos.LaTeX.WebApi.Services
{
    public class LaTeXServiceRequest
    {
        public string LaTeX { get; set; }

        public string TextColor { get; set; } = "000000";

        public string PageColor { get; set; } = "FFFFFF";

        public override bool Equals(object obj) {
            return obj is LaTeXServiceRequest request &&
                   LaTeX == request.LaTeX &&
                   TextColor == request.TextColor &&
                   PageColor == request.PageColor;
        }

        public override int GetHashCode() {
            return HashCode.Combine(LaTeX, TextColor, PageColor);
        }
    }
}
