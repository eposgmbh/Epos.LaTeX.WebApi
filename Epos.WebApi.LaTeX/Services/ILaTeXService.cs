namespace Epos.WebApi.LaTeX.Services
{
    public interface ILaTeXService
    {
        LaTeXServiceResponse GetPng(LaTeXServiceRequest request);
    }
}
