namespace Epos.LaTeX.WebApi.Services;

public interface IErrorImageService
{
    byte[] GetErrorImageFromMessage(string message);
}
