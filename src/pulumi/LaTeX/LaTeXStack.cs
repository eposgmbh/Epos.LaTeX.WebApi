using Epos.Kubernetes;

namespace LaTeX;

public class LaTeXStack : Stack
{
    public LaTeXStack() {
        _ = new LaTeXService();
    }
}
