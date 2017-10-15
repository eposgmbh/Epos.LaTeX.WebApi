namespace Epos.Blog.LaTeX.Services
{
    public class LaTeXServiceResponse
    {
        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }

        public byte[] PngImageData { get; set; }

        public long DurationMilliseconds { get; set; }
    }
}
