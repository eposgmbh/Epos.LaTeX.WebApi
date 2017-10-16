namespace Epos.LaTeX.WebApi.Services
{
    public interface ILaTeXService
    {
        LaTeXServiceResponse GetPng(LaTeXServiceRequest request);
    }
}
