namespace Epos.Blog.LaTeX.Services
{
    public interface ILaTeXService
    {
        LaTeXServiceResponse GetPng(LaTeXServiceRequest request);
    }
}
