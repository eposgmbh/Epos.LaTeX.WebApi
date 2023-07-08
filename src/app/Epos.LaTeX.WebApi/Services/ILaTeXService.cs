namespace Epos.LaTeX.WebApi.Services;

public interface ILaTeXService
{
    LaTeXServiceResponse GetArtifact(LaTeXServiceRequest request);
}
